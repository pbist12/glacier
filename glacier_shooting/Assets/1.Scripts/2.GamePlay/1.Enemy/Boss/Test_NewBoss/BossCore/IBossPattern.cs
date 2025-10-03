using System.Collections;

namespace BossSystem
{
    public interface IBossPattern
    {
        string Name { get; }
        bool Uninterruptible { get; } // true면 강제중단 불가
        float Weight { get; }         // 선택 가중치
        float Cooldown { get; }       // 선택 후 쿨다운(초)

        // 패턴 실행. 외부에서 StopCoroutine 시 인터럽트.
        IEnumerator Play(BossContext ctx);
    }
}
