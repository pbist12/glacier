using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropEntry
{
    [Tooltip("드롭될 프리팹")]
    public GameObject prefab;

    [Min(0f), Tooltip("상대적 가중치 (확률이 아님). 0이면 선택 대상에서 사실상 제외됩니다.")]
    public float weight = 1f;
}

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Rule")]
    [Range(0f, 1f), Tooltip("몬스터 처치 시 아이템을 드롭할 확률")]
    public float dropChance = 0.7f;

    [Tooltip("드롭 대상 아이템들(가중치 룰렛으로 1개 선택)")]
    public List<DropEntry> dropTable = new();

    /// <summary>
    /// 현재 트랜스폼 위치에 드롭 시도
    /// </summary>
    public void DropItem()
    {
        DropItemAt(transform.position);
    }

    /// <summary>
    /// 지정 위치에 드롭 시도: 1) dropChance 판정 → 2) 가중치로 아이템 1개 선택
    /// </summary>
    public void DropItemAt(Vector3 position)
    {
        // 1) 전체 드롭 확률 판정
        if (Random.value > dropChance) return;

        // 2) 가중치 합 계산 (prefab null 또는 weight<=0은 제외)
        float totalWeight = 0f;
        for (int i = 0; i < dropTable.Count; i++)
        {
            var e = dropTable[i];
            if (e == null || e.prefab == null || e.weight <= 0f) continue;
            totalWeight += e.weight;
        }

        // 유효한 항목이 없으면 드롭 스킵
        if (totalWeight <= 0f) return;

        // 3) 룰렛 선택
        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < dropTable.Count; i++)
        {
            var e = dropTable[i];
            if (e == null || e.prefab == null || e.weight <= 0f) continue;

            cumulative += e.weight;
            if (roll <= cumulative)
            {
                Instantiate(e.prefab, position, Quaternion.identity);
                return; // 한 개만 드롭
            }
        }
    }
}
