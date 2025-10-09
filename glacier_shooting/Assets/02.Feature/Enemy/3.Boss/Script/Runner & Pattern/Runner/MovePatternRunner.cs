using Boss;
using Game.Data;
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MovePatternRunner : MonoBehaviour, IPatternRunner, ISupportsPatternKind
{
    public PatternKind Kind => PatternKind.Move;
    public IEnumerator RunOnce(PatternSOBase so, BossRuntimeContext ctx, Func<bool> stop)
    {
        var m = (MovePatternSO)so;
        if (ctx.Boss == null) yield break;

        Vector3 start = ctx.Boss.position;
        Vector3 end = start + (Vector3)ctx.Boss.TransformDirection(m.localOffset);

        float dur = Mathf.Max(0.01f, so.actionSeconds);
        float t = 0f;

        while (t < dur && !stop())
        {
            t += ctx.DeltaTime();
            float u = Mathf.Clamp01(t / dur);

            // ✅ AnimationCurve가 없거나 키가 없으면 선형 진행으로 대체
            float k = (m.ease != null && m.ease.length > 0) ? m.ease.Evaluate(u) : u;

            ctx.Boss.position = Vector3.LerpUnclamped(start, end, k);
            yield return null;
        }
    }
}
