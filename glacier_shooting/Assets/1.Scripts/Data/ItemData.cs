using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("�з�/��Ÿ")]
    public ItemType itemType;
    public Rarity rarity;
    [Min(0)] public int price = 10;
    [Min(1)] public int stackLimit = 99;           // �Ҹ�ǰ ��
    public bool unique;                            // �ߺ� ���� ����(��: ����)

    [Header("��� ���")]
    public UseMode useMode = UseMode.Passive;      // Passive / OnUse / OnEquip
    [Min(0)] public float cooldown;                // OnUse�� �� ��ٿ�(����)
    public bool consumeOnPickup = false;           // �ݴ� ���� �ٷ� ���(On Use ����)

    [Header("ȿ���� (��������)")]
    [SerializeReference] public List<ItemEffect> effects = new(); // SO �߰� ���� Ȯ��!
}

public enum ItemType { Consumable, Equipment, Relic, Material, Quest, Special }
public enum Rarity { Common = 1, Uncommon, Rare, Epic, Legendary }
public enum UseMode { Passive, OnUse, OnEquip }
