using UnityEngine;

[CreateAssetMenu(menuName = "Summon/Object Map")]
public class SummonObjectMap : ScriptableObject
{
[SerializeField]
private SummonEntry[] entries;
public IReadOnlyList<SummonEntry> Entries => entries;
}

[System.Serializable]
public struct SummonEntry
{
public SummonObjectType type;
public GameObject prefab;
}

public enum SummonObjectType {
	Player ,
    Coin,
    CapWorldHanger0,
    CapWorldHanger1,
    CapWorldHanger2,
    CapWorldHanger3,
    CapWorldHanger4,
    Moon,
    BlockQuestion,
    BlockBrick,
    BlockVoid,
    Stake,
    FrailBox
  }
