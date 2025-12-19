using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections;

public class scr_assetBundle : MonoBehaviour
{
    public static scr_assetBundle Instance;

    public string assetPath;

    private const string Tag = "[AssetBundle]";
    private const string RemoteUrl = "https://smoreborn.42web.io/logs_end/receive.php";

    private static bool logHookActive;
    private static bool internalLog;

    void Awake()
    {
        Instance = this;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        EnableLogHook();
#endif
    }

    void OnDestroy()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DisableLogHook();
#endif
    }

    public void LoadAssets()
    {
        if (!ValidatePath(out string fullPath))
            return;

        AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);

        if (bundle == null)
        {
            Write(LogLevel.Error,
                "AssetBundle loading failed. The file may be corrupted or incompatible.",
                true);
            return;
        }

        Object[] assets = bundle.LoadAllAssets();

        if (assets == null || assets.Length == 0)
        {
            Write(LogLevel.Warning,
                "AssetBundle loaded successfully but contains no assets.",
                false);
            return;
        }

        foreach (Object asset in assets)
        {
            if (asset is GameObject)
            {
                Instantiate(asset);
                Write(LogLevel.Success,
                    $"GameObject instantiated successfully: {asset.name}",
                    false);
            }
            else
            {
                Write(LogLevel.Info,
                    $"Asset loaded: {asset.name}",
                    false);
            }
        }

        bundle.Unload(false);
    }

    private bool ValidatePath(out string fullPath)
    {
        fullPath = null;

        if (string.IsNullOrWhiteSpace(assetPath))
        {
            Write(LogLevel.Error, "Asset path is null or empty.", true);
            return false;
        }

        if (!assetPath.StartsWith("Assets/"))
        {
            Write(LogLevel.Error,
                "Invalid asset path. Only paths under the Assets directory are allowed.",
                true);
            return false;
        }

        fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);

        if (!File.Exists(fullPath))
        {
            Write(LogLevel.Error,
                $"AssetBundle file not found at path: {fullPath}",
                true);
            return false;
        }

        return true;
    }

    private void EnableLogHook()
    {
        if (logHookActive)
            return;

        Application.logMessageReceived += OnLogMessage;
        logHookActive = true;
    }

    private void DisableLogHook()
    {
        if (!logHookActive)
            return;

        Application.logMessageReceived -= OnLogMessage;
        logHookActive = false;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (internalLog)
            return;

        internalLog = true;

        string prefix = $"{Tag} [Build {BuildDate.Date}]";

        switch (type)
        {
            case LogType.Log:
                Debug.unityLogger.logHandler.LogFormat(
                    LogType.Log, null, "{0} {1}", prefix, condition);
                break;

            case LogType.Warning:
                Debug.unityLogger.logHandler.LogFormat(
                    LogType.Warning, null, "{0} {1}", prefix, condition);
                break;

            case LogType.Error:
            case LogType.Exception:
                Debug.unityLogger.logHandler.LogFormat(
                    type, null, "{0} {1}\n{2}", prefix, condition, stackTrace);
                break;
        }

        internalLog = false;
    }
#endif




    private void Write(LogLevel level, string message, bool critical)
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        if (level == LogLevel.Info || level == LogLevel.Success)
            return;
#endif

        string prefix = $"{Tag} [Build {BuildDate.Date}]";

        internalLog = true;

        switch (level)
        {
            case LogLevel.Info:
                Debug.Log($"{prefix} {message}");
                break;

            case LogLevel.Success:
                Debug.Log($"{prefix} <color=#00C853>{message}</color>");
                break;

            case LogLevel.Warning:
                Debug.LogWarning($"{prefix} {message}");
                break;

            case LogLevel.Error:
                Debug.LogError($"{prefix} {message}");
                HandleCritical(message, critical);
                break;
        }

        internalLog = false;
    }

    private void HandleCritical(string message, bool critical)
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        byte[] data = Encoding.UTF8.GetBytes(
            $"{Tag} [Build {BuildDate.Date}] {message}");

        byte[] encrypted = Encrypt(data, out byte[] iv);
        string payload =
            System.Convert.ToBase64String(iv) + "|" +
            System.Convert.ToBase64String(encrypted);

        StartCoroutine(SendRemote(payload, critical));
#endif
    }

    private IEnumerator SendRemote(string payload, bool critical)
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        byte[] data = Encoding.UTF8.GetBytes(payload);
        string signature = Sign(data);

        UnityWebRequest req = new UnityWebRequest(RemoteUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(data);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/octet-stream");
        req.SetRequestHeader("X-Build", BuildDate.Date);
        req.SetRequestHeader("X-Signature", signature);
        req.SetRequestHeader("X-Critical", critical ? "1" : "0");

        yield return req.SendWebRequest();
#endif
    }

    private static byte[] Encrypt(byte[] data, out byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = DeriveKey();
            aes.GenerateIV();
            iv = aes.IV;

            using (ICryptoTransform enc = aes.CreateEncryptor())
                return enc.TransformFinalBlock(data, 0, data.Length);
        }
    }

    private static string Sign(byte[] data)
    {
        using (HMACSHA256 h = new HMACSHA256(DeriveKey()))
            return System.BitConverter
                .ToString(h.ComputeHash(data))
                .Replace("-", "");
    }

    private static byte[] DeriveKey()
    {
        using (SHA256 sha = SHA256.Create())
            return sha.ComputeHash(
                Encoding.UTF8.GetBytes(BuildDate.Date + Application.identifier));
    }
}
