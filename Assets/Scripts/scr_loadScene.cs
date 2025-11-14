using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scr_loadScene : MonoBehaviour
{
	[HideInInspector] public static scr_loadScene s;
	public static scr_loadScene Instance { get; private set; }

	[HideInInspector] public string nextScene = "scn_menuTitle";
	[HideInInspector] public bool isDone = false;
	private AsyncOperation _loadOP;

	public enum TransitionType
	{
		Direct = 0,
		Ship = 1,
		CapFly = 2,
		Async = 3
	}

    private void Start()
	{
		// Inicialización del singleton
		Instance = this;
		s = this; // Compatibilidad hacia atrás
	}

	public void StartScene(string sceneName, int transition = 0)
	{
		isDone = false;
		ConfigureSceneStart(sceneName);

		TransitionType t = (TransitionType)Mathf.Clamp(transition, 0, System.Enum.GetNames(typeof(TransitionType)).Length - 1);
		ExecuteTransition(t);
	}

	private void ConfigureSceneStart(string sceneName)
	{
		scr_main.s.hasLevelLoaded = false;
		nextScene = sceneName;
		scr_main.s.dbg_enemyCount = 0;
		scr_main.DPrint("nSCN: " + nextScene);
	}

	private void ExecuteTransition(TransitionType t)
	{
		switch (t)
		{
			case TransitionType.Direct:
				// Carga directa sin transición
				SceneManager.LoadScene(nextScene);
				break;

			case TransitionType.Ship:
				// Transición: cargar escena intermedia del barco
				scr_main.s.SetFocus(false);
				SceneManager.LoadScene("scn_loadShip");
				// nextScene ya establecido
				break;

			case TransitionType.CapFly:
				// Activar objeto de transición (asegurar que el índice exista)
				Transform tParent = scr_main.s.transform;
				if (tParent.childCount > 1 && tParent.GetChild(1).childCount > 2)
				{
					tParent.GetChild(1).GetChild(2).gameObject.SetActive(true);
				}
				else
				{
					scr_main.DPrint("CapFly transition: estructura esperada no encontrada.", false);
				}
				break;

			case TransitionType.Async:
				StartAsyncLoad();
				break;
		}
	}

	private void StartAsyncLoad()
	{
		StartCoroutine(LoadAsync());
	}

	private IEnumerator LoadAsync()
	{
		if (!PrepareAsyncLoad())
			yield break;

		while (!_loadOP.isDone)
		{
			ReportAsyncProgress();
			yield return null;
		}

		FinalizeAsyncLoad();
	}

	private bool PrepareAsyncLoad()
	{
		if (string.IsNullOrEmpty(nextScene))
		{
			scr_main.DPrint("LoadAsync: nextScene no está configurada.", false);
			return false;
		}

		_loadOP = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
		_loadOP.allowSceneActivation = false;
		return true;
	}

	private void ReportAsyncProgress()
	{
		float scaledProgress = (_loadOP.progress < 0.9f) ? (_loadOP.progress / 0.9f) : 1f;
		int percent = Mathf.RoundToInt(scaledProgress * 100f);
		scr_main.DPrint("loading: " + percent + "%", false);
	}

	private void FinalizeAsyncLoad()
	{
		scr_main.DPrint("loading: 100%");
		isDone = true;

		Scene loaded = SceneManager.GetSceneByName(nextScene);
		if (loaded.IsValid())
		{
			SceneManager.SetActiveScene(loaded);
		}
		else
		{
			scr_main.DPrint("LoadAsync: escena cargada no es válida: " + nextScene, false);
		}
	}

	public void SetSceneActive()
	{
		if (_loadOP == null) {
			scr_main.DPrint("SetSceneActive: no hay operación de carga en curso.", false);
			return;
		}
		_loadOP.allowSceneActivation = true;
	}
}
