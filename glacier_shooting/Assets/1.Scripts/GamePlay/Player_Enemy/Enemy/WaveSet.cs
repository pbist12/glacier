using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Wave/WaveSet")]
public class WaveSet : ScriptableObject
{
    [Min(0.1f)] public float periodN = 10f;   // N초 (웨이브 주기)
    public bool loop;                          // 끝나면 다시 처음으로?
    public List<WaveDef> waves = new();        // 1번 웨이브부터 순서대로
}

[Serializable]
public class WaveDef
{
    public string waveName = "Wave";
    public List<SpawnGroup> groups = new();    // 같은 웨이브 내에서 여러 그룹(종류/스폰포인트/간격)
}

[Serializable]
public class SpawnGroup
{
    public GameObject enemyPrefab;             // 몬스터 프리팹
    public Transform[] spawnPoints;            // 하나 이상 지정 시 랜덤/순환
    public int count = 5;                      // 몇 마리
    [Min(0f)] public float startDelay = 0f;    // 웨이브 시작으로부터 지연
    [Min(0f)] public float interval = 0.2f;    // 마리 간 간격
    public bool cycleSpawnPoints = true;       // true면 0,1,2… 순환; false면 랜덤
}
