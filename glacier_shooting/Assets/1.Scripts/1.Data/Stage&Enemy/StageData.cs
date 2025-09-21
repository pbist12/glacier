using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "GameMini/StageData")]
public class StageData : ScriptableObject
{
    [Header("Mobs: ���ڸ�ŭ Pool ���� ����")]
    public List<MobEntry> mobs = new();

    [Header("Elite")]
    public List<EliteEntry> elites = new();

    [Header("Bosses: ������� ����")]
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