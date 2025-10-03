using UnityEngine;

namespace BossSystem
{
    [CreateAssetMenu(menuName = "Boss/Phase")]
    public class BossPhase : ScriptableObject
    {
        [Range(0f, 1f)] public float endHpRatio = 0.7f; // �� ���� ���ϰ� �Ǹ� ���� ������� �Ѿ
        public PatternAsset[] patterns;

        [Tooltip("���� ���� �⺻ ����(��)")]
        public Vector2 idleBetweenPatterns = new Vector2(0.5f, 1.0f);
    }
}
