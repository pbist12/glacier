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

    // 아이템 추가
    public void Add(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return;
        var e = items.Find(x => x.item == item);
        if (e == null)
        {
            items.Add(new Entry { item = item, amount = amount });
        }
        else
        {
            e.amount += amount;
        }
    }

    // 아이템 제거(성공시 true)
    public bool Remove(ItemData item, int amount)
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
}
