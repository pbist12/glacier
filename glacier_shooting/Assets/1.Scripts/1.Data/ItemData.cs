using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("분류/메타")]
    public ItemType itemType;
    public Rarity rarity;
    [Min(0)] public int price = 10;
    [Min(1)] public int stackLimit = 99;           // 소모품 등
    public bool unique;                            // 중복 보유 금지(예: 유물)

    [Header("사용 방식")]
    public UseMode useMode = UseMode.Passive;      // Passive / OnUse / OnEquip
    [Min(0)] public float cooldown;                // OnUse일 때 쿨다운(선택)
    public bool consumeOnPickup = false;           // 줍는 순간 바로 사용(On Use 전용)

    [Header("효과들 (폴리모픽)")]
    [SerializeReference] public List<ItemEffect> effects = new(); // SO 추가 없이 확장!
}

public enum ItemType { Consumable, Equipment, Relic, Material, Quest, Special }
public enum Rarity { Common = 1, Uncommon, Rare, Epic, Legendary }
public enum UseMode { Passive, OnUse, OnEquip }
