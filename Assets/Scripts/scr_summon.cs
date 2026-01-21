using System.Collections.Generic;
using UnityEngine;

public class scr_summon : MonoBehaviour {
	
	public static scr_summon Instance { get; private set; }

    [SerializeField] private SummonObjectMap objectMap;
    private Dictionary<SummonObjectType, GameObject> objectCache;

	void Awake()
	{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    BuildCache();
	}

	private void BuildCache()
	{
    if (objectMap == null)
    {
        Debug.LogError("SummonObjectMap no asignado en scr_summon");
        return;
    }

    objectCache = new Dictionary<SummonObjectType, GameObject>();

    foreach (var entry in objectMap.Entries)
    {
        if (entry.prefab == null)
        {
            Debug.LogWarning("Prefab nulo para: " + entry.type);
            continue;
        }

        if (!objectCache.ContainsKey(entry.type))
            objectCache.Add(entry.type, entry.prefab);
    }
	}

	//No es necesario esta lineas de codigos, pero lo dejo comentado por si alguien quiere revisar (SCRIPT REMOVIDO POR MARKUT)
    /*public GameObject SpawnPlayer(Vector3 pos, Vector3 rot)
	{
    if (MarioController.s != null)
        return null;

    return Instantiate(
        Resources.Load<GameObject>("Objects/objMario"),
        pos,
        Quaternion.Euler(rot)
    );
	}*/
	
    public GameObject SpawnObject(SummonObjectType type, Vector3 pos, Vector3 rot)
	{
    if (objectCache == null)
    {
        Debug.LogError("Cache de objetos no inicializado");
        return null;
    }

    else if (!objectCache.TryGetValue(type, out GameObject prefab))
    {
        Debug.LogError("Objeto no registrado: " + type);
        return null;
    }

    return Instantiate(prefab, pos, Quaternion.Euler(rot));
	}
	
}
