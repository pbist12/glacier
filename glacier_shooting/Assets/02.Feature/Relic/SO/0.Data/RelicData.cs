using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "Game/Relic")]
public class RelicData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string RelicName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("�з�/��Ÿ")]
    public Rarity rarity;
    [Min(0)] public int price = 10;

    [Header("��� ���")]
    [Min(0)] public float cooldown;                // OnUse�� �� ��ٿ�(����)

    [Header("ȿ���� (��������)")]
    [SerializeReference] public List<ItemEffect> effects = new(); // SO �߰� ���� Ȯ��!
}

public enum Rarity { Common = 1, Uncommon, Rare, Epic, Legendary }
