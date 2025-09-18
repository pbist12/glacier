using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("돈")]
    [Min(0)] public int gold = 100;
    [Header("폭탄")]
    [Min(0)] public int bomb = 10;

    [Header("플레이어 UI")]
    public PlayerUI playerUI;

    [System.Serializable]
    public class Entry
    {
        public RelicData item;
    }

    [Header("아이템 보유 목록")]
    public List<Entry> playerRelics = new(); // 초간단: 중복 허용 X, 같은 아이템은 수량 합침

    private void Awake()
    {
        if (!playerUI) playerUI = FindFirstObjectByType<PlayerUI>();
    }

    private void Update()
    {
        playerUI.RefreshItem(gold,bomb);
    }


    #region Relic
    public void AddRelicToInventory(RelicData item)
    {
        if (item == null) return;
        var e = playerRelics.Find(x => x.item == item);
        if (e == null)
        {
            playerRelics.Add(new Entry { item = item});
            ApplyRelic(item);
        }
    }
    public bool RemoveFromInventory(RelicData item)
    {
        if (item == null) return false;

        var e = playerRelics.Find(x => x.item == item);
        if (e == null) return false;

        playerRelics.Remove(e);
        return true;
    }


    /// <summary>
    /// 아이템 적용 (OnUse/Passive/장비 착용 시 등)
    /// </summary>
    public void ApplyRelic(RelicData item)
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
    public void RemoveRelic(RelicData item)
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
    #endregion
}
