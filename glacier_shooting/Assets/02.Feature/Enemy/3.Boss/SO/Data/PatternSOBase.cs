using UnityEngine;

namespace Game.Data
{
    public enum PatternKind
    {
        Spread,   // ��ä��/�� ���� �迭��
        Dash,     // ���� (�߰� ����)
        Spiral,   // �����̷� (�߰� ����)
        Laser,    // ������ (�߰� ����)
        Summon,   // ��ȯ (�߰� ����)
        Move,
    }

    /// <summary>
    /// ���� SO ���� ��Ÿ�� ����(���� ������ ��Ÿ�� �����ٷ�/����� ���).
    /// </summary>
    public abstract class PatternSOBase : ScriptableObject
    {
        [Header("Meta")]
        public string patternId = "PATTERN";
        [TextArea] public string note;
        public PatternKind kind = PatternKind.Spread;

        [Header("Telegraph / Action / Post")]
        [Min(0f)] public float telegraphSeconds = 0.5f; // ����
        [Min(0f)] public float actionSeconds = 1.2f; // ���� ���� ���� �ð�
        [Min(0f)] public float postDelaySeconds = 0.2f; // �ĵ�
    }

}
