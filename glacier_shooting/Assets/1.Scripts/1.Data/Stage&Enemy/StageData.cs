using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "GameMini/StageData")]
public class StageData : ScriptableObject
{
    [Header("Mobs: 숫자만큼 Pool 에서 스폰")]
    public List<MobEntry> mobs = new();

    [Header("Elite")]
    public List<EliteEntry> elites = new();

    [Header("Bosses: 순서대로 진행")]
    public List<BossAsset> bosses = new();
}

[System.Serializable]
public class MobEntry
{
    public GameObject prefab;
    public int spawnCount;
}

[System.Serializable]
public class EliteEntry
{
    public GameObject prefab;
}