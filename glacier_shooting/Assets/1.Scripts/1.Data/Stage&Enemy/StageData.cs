using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "GameMini/StageData")]
public class StageData : ScriptableObject
{
    public enum SpawnPoints { None, Right, Left, Up, CenterTop }

    [Min(0.1f)] public float periodN = 10f;
    public bool loop;

    [Header("Mobs: 숫자만큼 Pool 에서 스폰")]
    public WaveDef waves = new();

    [Header("Elite (후보들, 1마리 랜덤 사용)")]
    public List<EliteAsset> elites = new();

    [Header("Bosses (목록 중 1마리 랜덤 선택)")]
    public List<BossAsset> bosses = new();

#if UNITY_EDITOR
    // 기존 SO에서 spawnCount가 0으로 남는 경우를 대비해 보정
    void OnValidate()
    {
        if (waves != null && waves.groups != null)
        {
            foreach (var g in waves.groups)
                if (g != null && g.spawnCount < 1) g.spawnCount = 1;
        }
    }
#endif
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
    public MobAsset monster;                        // 몬스터 프리팹(or SO)
    public StageData.SpawnPoints[] spawnPoints;     // 지정 시 랜덤/순환
    [Min(0f)] public float startDelay = 0f;         // 웨이브 시작 기준 지연
    [Min(0f)] public float interval = 0.2f;         // 마리 간 간격
    public bool cycleSpawnPoints = true;            // true: 순환 / false: 랜덤

    [Min(1)] public int spawnCount = 1;             // 🔹 추가: 이 수만큼 생성
}
#endregion
