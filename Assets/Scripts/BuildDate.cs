using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public static class BuildDate
{
    public const string Date = "Unknown";
    private const string ClientFile = "Client version.txt";
    private const string VerifyUrl = "http://smoreborn.42web.io/verify.php";

    static BuildDate()
    {
#if !UNITY_EDITOR
        try
        {
            if (Debugger.IsAttached)
                Environment.FailFast(string.Empty);

            ValidateEnvironment();
            ValidateClient();
            ValidateIntegrity();
            ValidateBinary();
            ValidateHardware();
            VerifyExternal();
        }
        catch
        {
            Environment.FailFast(string.Empty);
        }
#endif
    }

    private static void ValidateEnvironment()
    {
        string n = Environment.MachineName.ToLowerInvariant();
        if (n.Contains("vm") || n.Contains("vbox") || n.Contains("xen") || n.Contains("qemu"))
            Environment.FailFast(string.Empty);

        foreach (var p in Process.GetProcesses())
        {
            try
            {
                string name = p.ProcessName.ToLowerInvariant();
                if (name.Contains("cheat") || name.Contains("scan") || name.Contains("engine"))
                    Environment.FailFast(string.Empty);
            }
            catch { }
        }
    }

    private static void ValidateClient()
    {
        if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClientFile)))
            Environment.FailFast(string.Empty);
    }

    private static void ValidateIntegrity()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        string[] l = File.ReadAllLines(Path.Combine(root, ClientFile));
        if (l.Length < 7)
            Environment.FailFast(string.Empty);

        string payload = l[0] + "\n" + l[1] + "\n" + l[2];
        if (Extract(l[3]) != ComputeHmac(payload))
            Environment.FailFast(string.Empty);

        if (Extract(l[4]) != ComputeFilesHash(root))
            Environment.FailFast(string.Empty);
    }

    private static void ValidateBinary()
    {
        string exe = Process.GetCurrentProcess().MainModule.FileName;
        string hash = HashFile(exe);
        if (Extract(ReadLine(5)) != hash)
            Environment.FailFast(string.Empty);
    }

    private static void ValidateHardware()
    {
        string hw = ComputeHmac(
            Environment.ProcessorCount +
            Environment.OSVersion.VersionString +
            Environment.UserDomainName);

        if (Extract(ReadLine(6)) != hw)
            Environment.FailFast(string.Empty);
    }

    private static void VerifyExternal()
    {
        try
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add("X-Client", ComputeHmac(Date));
                wc.DownloadString(VerifyUrl);
            }
        }
        catch { }
    }

#if UNITY_EDITOR

    [MenuItem("Tools/Build Game")]
    private static void Open()
    {
        GetWindow<BuildWindow>("Build Game");
    }

    private sealed class BuildWindow : EditorWindow
    {
        private void OnGUI()
        {
            GUILayout.Label("Build Tool", EditorStyles.boldLabel);
            GUILayout.Space(8);
            if (GUILayout.Button("Build Game"))
                Execute();
        }

        private void Execute()
        {
            string folder = EditorUtility.SaveFolderPanel("Select Build Folder", "", "");
            if (string.IsNullOrEmpty(folder))
                return;

            string date = DateTime.Now.ToString("MMM dd yyyy HH:mm:ss");
            RewriteSelf(date);

            BuildPlayerOptions o = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = folder,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            var r = BuildPipeline.BuildPlayer(o);
            if (r.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                return;

            WriteClient(folder, date);
        }

        private static void WriteClient(string folder, string date)
        {
            string l1 = "Unity Version: " + Application.unityVersion;
            string l2 = "Game Version: " + PlayerSettings.bundleVersion;
            string l3 = "Build Date: " + date;
            string payload = l1 + "\n" + l2 + "\n" + l3;

            File.WriteAllText(
                Path.Combine(folder, ClientFile),
                payload +
                "\nChecksum: " + ComputeHmac(payload) +
                "\nFiles Hash: " + ComputeFilesHash(folder) +
                "\nBinary Hash: " + HashFile(EditorApplication.applicationPath) +
                "\nHardware Sig: " + ComputeHmac(
                    Environment.ProcessorCount +
                    Environment.OSVersion.VersionString +
                    Environment.UserDomainName)
            );
        }

        private static void RewriteSelf(string date)
        {
            string p = AssetDatabase.GUIDToAssetPath(
                AssetDatabase.FindAssets("BuildDate t:Script")[0]);

            File.WriteAllText(p,
$@"using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public static class BuildDate
{{
    public const string Date = ""{date}"";
    private const string ClientFile = ""Client version.txt"";
    private const string VerifyUrl = ""{VerifyUrl}"";

{RuntimeBlock()}
}}");

            AssetDatabase.Refresh();
        }

        private static string RuntimeBlock()
        {
            return @"
    static BuildDate()
    {
#if !UNITY_EDITOR
        try
        {
            if (Debugger.IsAttached)
                Environment.FailFast(string.Empty);

            ValidateEnvironment();
            ValidateClient();
            ValidateIntegrity();
            ValidateBinary();
            ValidateHardware();
            VerifyExternal();
        }
        catch
        {
            Environment.FailFast(string.Empty);
        }
#endif
    }";
        }
    }

#endif

    private static string ComputeHmac(string v)
    {
        using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(Key())))
            return Hex(h.ComputeHash(Encoding.UTF8.GetBytes(v)));
    }

    private static string ComputeFilesHash(string r)
    {
        using (var s = SHA256.Create())
        {
            foreach (var f in Directory.GetFiles(r, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(ClientFile)))
            {
                byte[] d = File.ReadAllBytes(f);
                s.TransformBlock(d, 0, d.Length, null, 0);
            }
            s.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return Hex(s.Hash);
        }
    }

    private static string HashFile(string p)
    {
        using (var s = SHA256.Create())
            return Hex(s.ComputeHash(File.ReadAllBytes(p)));
    }

    private static string ReadLine(int i)
    {
        return File.ReadAllLines(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ClientFile))[i];
    }

    private static string Extract(string l)
    {
        return l.Substring(l.IndexOf(':') + 1).Trim();
    }

    private static string Hex(byte[] d)
    {
        StringBuilder b = new StringBuilder(d.Length * 2);
        foreach (byte x in d)
            b.Append(x.ToString("x2"));
        return b.ToString();
    }

    private static string Key()
    {
        return string.Concat("B8F2", "A9D3", "_X7", "91K");
    }
}
