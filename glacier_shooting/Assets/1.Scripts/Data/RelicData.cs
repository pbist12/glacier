using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "Game/Relic")]
public class RelicData : ScriptableObject
{
    [Header("기본 정보")]
    public string relicName;       // 유물 이름
    [TextArea] public string description; // 설명
    public Sprite icon;            // UI 표시용 아이콘

    [Header("게임 효과")]
    public RelicType relicType;    // 유물의 종류 (공격/방어/특수 등)
    public int power;              // 공격력/효과량
    public float multiplier = 1f;  // 배율

    [Header("조건/메타")]
    public int rarity;             // 희귀도 (1=일반, 5=전설)
}

public enum RelicType
{
    Attack,
    Defense,
    Utility,
    Special
}
