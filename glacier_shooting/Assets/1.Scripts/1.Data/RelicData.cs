using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "Game/Relic")]
public class RelicData : ScriptableObject
{
    [Header("기본 정보")]
    public string RelicName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("분류/메타")]
    public Rarity rarity;
    [Min(0)] public int price = 10;

    [Header("사용 방식")]
    [Min(0)] public float cooldown;                // OnUse일 때 쿨다운(선택)

    [Header("효과들 (폴리모픽)")]
    [SerializeReference] public List<ItemEffect> effects = new(); // SO 추가 없이 확장!
}

public enum Rarity { Common = 1, Uncommon, Rare, Epic, Legendary }
