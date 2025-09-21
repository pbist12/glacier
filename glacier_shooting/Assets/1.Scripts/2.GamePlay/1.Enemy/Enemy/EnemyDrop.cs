using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropEntry
{
    [Tooltip("��ӵ� ������")]
    public GameObject prefab;

    [Min(0f), Tooltip("����� ����ġ (Ȯ���� �ƴ�). 0�̸� ���� ��󿡼� ��ǻ� ���ܵ˴ϴ�.")]
    public float weight = 1f;
}

public class EnemyDrop : MonoBehaviour
{
    [Header("Drop Rule")]
    [Range(0f, 1f), Tooltip("���� óġ �� �������� ����� Ȯ��")]
    public float dropChance = 0.7f;

    [Tooltip("��� ��� �����۵�(����ġ �귿���� 1�� ����)")]
    public List<DropEntry> dropTable = new();

    /// <summary>
    /// ���� Ʈ������ ��ġ�� ��� �õ�
    /// </summary>
    public void DropItem()
    {
        DropItemAt(transform.position);
    }

    /// <summary>
    /// ���� ��ġ�� ��� �õ�: 1) dropChance ���� �� 2) ����ġ�� ������ 1�� ����
    /// </summary>
    public void DropItemAt(Vector3 position)
    {
        // 1) ��ü ��� Ȯ�� ����
        if (Random.value > dropChance) return;

        // 2) ����ġ �� ��� (prefab null �Ǵ� weight<=0�� ����)
        float totalWeight = 0f;
        for (int i = 0; i < dropTable.Count; i++)
        {
            var e = dropTable[i];
            if (e == null || e.prefab == null || e.weight <= 0f) continue;
            totalWeight += e.weight;
        }

        // ��ȿ�� �׸��� ������ ��� ��ŵ
        if (totalWeight <= 0f) return;

        // 3) �귿 ����
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
                return; // �� ���� ���
            }
        }
    }
}
