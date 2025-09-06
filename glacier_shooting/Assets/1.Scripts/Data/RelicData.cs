using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "Game/Relic")]
public class RelicData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string relicName;       // ���� �̸�
    [TextArea] public string description; // ����
    public Sprite icon;            // UI ǥ�ÿ� ������

    [Header("���� ȿ��")]
    public RelicType relicType;    // ������ ���� (����/���/Ư�� ��)
    public int power;              // ���ݷ�/ȿ����
    public float multiplier = 1f;  // ����

    [Header("����/��Ÿ")]
    public int rarity;             // ��͵� (1=�Ϲ�, 5=����)
}

public enum RelicType
{
    Attack,
    Defense,
    Utility,
    Special
}
