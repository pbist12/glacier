using Boss;
using Game.Data;
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class LaserPatternRunner : MonoBehaviour, IPatternRunner, ISupportsPatternKind
{
    public PatternKind Kind => PatternKind.Laser;
    public IEnumerator RunOnce(PatternSOBase so, BossRuntimeContext ctx, Func<bool> stop)
    {
        var l = (LaserPatternSO)so;
        if (ctx.Laser == null)
        {
            yield return Wait(so.actionSeconds, ctx, stop);
            yield break;
        }

        // 1) 차지(선택)
        if (l.chargeSeconds > 0f)
            yield return Wait(l.chargeSeconds, ctx, stop);

        // 2) 빔 ON
        ctx.Laser.SetWidth(l.beamWidth);
        ctx.Laser.Enable(true);

        float t = 0f, dur = Mathf.Max(0f, so.actionSeconds);
        while (t < dur && !stop())
        {
            t += ctx.DeltaTime();
            if (l.sweepDegPerSec != 0f)
                ctx.Laser.Sweep(l.sweepDegPerSec * ctx.DeltaTime()); // 구현에 맞게 회전/스윕
            yield return null;
        }

        // 3) OFF
        ctx.Laser.Enable(false);
    }

    static IEnumerator Wait(float sec, BossRuntimeContext ctx, Func<bool> stop)
    {
        float t = 0f; float d = Mathf.Max(0f, sec);
        while (t < d && !stop()) { t += ctx.DeltaTime(); yield return null; }
    }
}
