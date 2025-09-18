using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("��")]
    [Min(0)] public int gold = 100;
    [Header("��ź")]
    [Min(0)] public int bomb = 10;

    [Header("�÷��̾� UI")]
    public PlayerUI playerUI;

    [System.Serializable]
    public class Entry
    {
        public RelicData item;
    }

    [Header("������ ���� ���")]
    public List<Entry> playerRelics = new(); // �ʰ���: �ߺ� ��� X, ���� �������� ���� ��ħ

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
    /// ������ ���� (OnUse/Passive/��� ���� �� ��)
    /// </summary>
    public void ApplyRelic(RelicData item)
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
    public void RemoveRelic(RelicData item)
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
    #endregion
}
