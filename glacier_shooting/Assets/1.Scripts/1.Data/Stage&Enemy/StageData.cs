using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "GameMini/StageData")]
public class StageData : ScriptableObject
{
    public enum SpawnPoints { None, Right, Left, Up }

    [Min(0.1f)] public float periodN = 10f;   // N��(���̺� �� ����)
    public bool loop;                          // ���� ��Ģ: �� ���� �� Elite��Boss�� ����

    [Header("Mobs: ���ڸ�ŭ Pool ���� ����")]
    public WaveDef waves = new();

    [Header("Elite (�ĺ���, 1���� ���� ���)")]
    public List<EliteAsset> elites = new();

    [Header("Bosses (��� �� 1���� ���� ����)")]
    public List<BossAsset> bosses = new();
}

#region Normal Monster Prefab
[Serializable]
public class WaveDef
{
    public string waveID = "Wave";
    public List<SpawnGroup> groups = new();
}

[Serializable]
public class SpawnGroup
{
    public MobAsset monster;                        // ���� ������
    public StageData.SpawnPoints[] spawnPoints;     // ���� �� ����/��ȯ
    [Min(0f)] public float startDelay = 0f;         // ���̺� ���� ���� ����
    [Min(0f)] public float interval = 0.2f;         // ���� �� ����
    public bool cycleSpawnPoints = true;            // true: ��ȯ / false: ����
}
#endregion

