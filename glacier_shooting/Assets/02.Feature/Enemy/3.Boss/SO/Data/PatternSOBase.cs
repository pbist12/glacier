using UnityEngine;

namespace Game.Data
{
    public enum PatternKind
    {
        Spread,   // 부채꼴/팔 벌린 배열형
        Dash,     // 돌진 (추가 예정)
        Spiral,   // 스파이럴 (추가 예정)
        Laser,    // 레이저 (추가 예정)
        Summon,   // 소환 (추가 예정)
        Move,
    }

    /// <summary>
    /// 패턴 SO 공통 메타만 보관(실행 로직은 런타임 스케줄러/모듈이 담당).
    /// </summary>
    public abstract class PatternSOBase : ScriptableObject
    {
        [Header("Meta")]
        public string patternId = "PATTERN";
        [TextArea] public string note;
        public PatternKind kind = PatternKind.Spread;

        [Header("Telegraph / Action / Post")]
        [Min(0f)] public float telegraphSeconds = 0.5f; // 예고
        [Min(0f)] public float actionSeconds = 1.2f; // 실제 실행 유지 시간
        [Min(0f)] public float postDelaySeconds = 0.2f; // 후딜
    }

}
