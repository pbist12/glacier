using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("돈")]
    [Min(0)] public int gold = 100;

    [System.Serializable]
    public class Entry
    {
        public ItemData item;
        [Min(0)] public int amount = 1;
    }

    [Header("아이템 보유 목록")]
    public List<Entry> items = new(); // 초간단: 중복 허용 X, 같은 아이템은 수량 합침

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
    /// 아이템 적용 (OnUse/Passive/장비 착용 시 등)
    /// </summary>
    public void ApplyItem(ItemData item)
    {
        var ctx = new ItemContext(
            owner: PlayerStatus.Instance.gameObject,
            inventory: this,
            stats: PlayerStatus.Instance, // IPlayerStats 구현 → StatModifierEffect 등이 이쪽으로 들어옴
            logger: Debug.Log
        );

        foreach (var effect in item.effects)
        {
            effect?.Apply(ctx);
        }

        PlayerStatus.Instance.SetStat();
    }

    /// <summary>
    /// 아이템 해제(장비 해제/패시브 오프 등)
    /// </summary>
    public void RemoveItem(ItemData item)
    {
        var ctx = new ItemContext(
            owner: PlayerStatus.Instance.gameObject,
            inventory: this,
            stats: PlayerStatus.Instance,
            logger: Debug.Log
        );

        // 효과 제거 (revertOnRemove=true 인 효과들이 복원)
        for (int i = item.effects.Count - 1; i >= 0; i--)
            item.effects[i]?.Remove(ctx);

        PlayerStatus.Instance.SetStat();
    }
}
