using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("��")]
    [Min(0)] public int gold = 100;

    [System.Serializable]
    public class Entry
    {
        public ItemData item;
        [Min(0)] public int amount = 1;
    }

    [Header("������ ���� ���")]
    public List<Entry> items = new(); // �ʰ���: �ߺ� ��� X, ���� �������� ���� ��ħ

    public void AddToInventory(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return;
        var e = items.Find(x => x.item == item);
        if (e == null)
        {
            items.Add(new Entry { item = item, amount = amount });
            ApplyItem(item);
        }
        else
        {
            e.amount += amount;
        }
    }

    public bool RemoveFromInventory(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        var e = items.Find(x => x.item == item);
        if (e == null || e.amount < amount) return false;

        e.amount -= amount;
        if (e.amount <= 0) items.Remove(e);
        return true;
    }

    public int Count(ItemData item)
    {
        var e = items.Find(x => x.item == item);
        return e == null ? 0 : e.amount;
    }

    /// <summary>
    /// ������ ���� (OnUse/Passive/��� ���� �� ��)
    /// </summary>
    public void ApplyItem(ItemData item)
    {
        var ctx = new ItemContext(
            owner: PlayerStatus.Instance.gameObject,
            inventory: this,
            stats: PlayerStatus.Instance, // IPlayerStats ���� �� StatModifierEffect ���� �������� ����
            logger: Debug.Log
        );

        foreach (var effect in item.effects)
        {
            effect?.Apply(ctx);
        }

        PlayerStatus.Instance.SetStat();
    }

    /// <summary>
    /// ������ ����(��� ����/�нú� ���� ��)
    /// </summary>
    public void RemoveItem(ItemData item)
    {
        var ctx = new ItemContext(
            owner: PlayerStatus.Instance.gameObject,
            inventory: this,
            stats: PlayerStatus.Instance,
            logger: Debug.Log
        );

        // ȿ�� ���� (revertOnRemove=true �� ȿ������ ����)
        for (int i = item.effects.Count - 1; i >= 0; i--)
            item.effects[i]?.Remove(ctx);

        PlayerStatus.Instance.SetStat();
    }
}
