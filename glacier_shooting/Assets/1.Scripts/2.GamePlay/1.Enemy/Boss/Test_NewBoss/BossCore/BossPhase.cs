using UnityEngine;

namespace BossSystem
{
    [CreateAssetMenu(menuName = "Boss/Phase")]
    public class BossPhase : ScriptableObject
    {
        [Range(0f, 1f)] public float endHpRatio = 0.7f; // 이 비율 이하가 되면 다음 페이즈로 넘어감
        public PatternAsset[] patterns;

        [Tooltip("패턴 사이 기본 휴지(초)")]
        public Vector2 idleBetweenPatterns = new Vector2(0.5f, 1.0f);
    }
}
