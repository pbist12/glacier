using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Wave/WaveSet")]
public class WaveSet : ScriptableObject
{
    [Min(0.1f)] public float periodN = 10f;   // N�� (���̺� �ֱ�)
    public bool loop;                          // ������ �ٽ� ó������?
    public List<WaveDef> waves = new();        // 1�� ���̺���� �������
}

[Serializable]
public class WaveDef
{
    public string waveName = "Wave";
    public List<SpawnGroup> groups = new();    // ���� ���̺� ������ ���� �׷�(����/��������Ʈ/����)
}

[Serializable]
public class SpawnGroup
{
    public GameObject enemyPrefab;             // ���� ������
    public Transform[] spawnPoints;            // �ϳ� �̻� ���� �� ����/��ȯ
    public int count = 5;                      // �� ����
    [Min(0f)] public float startDelay = 0f;    // ���̺� �������κ��� ����
    [Min(0f)] public float interval = 0.2f;    // ���� �� ����
    public bool cycleSpawnPoints = true;       // true�� 0,1,2�� ��ȯ; false�� ����
}
