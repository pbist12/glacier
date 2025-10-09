// File: SpreadPatternRunner.cs
using Boss;
using Game.Data;
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SpreadPatternRunner : MonoBehaviour, IPatternRunner
{
    public bool useInternalShooterLoop = true; // true: isFire 스위치형, false: Runner가 FireOnce 펄스 직접 호출

    public IEnumerator RunOnce(PatternSOBase so, BossRuntimeContext ctx, Func<bool> stop)
    {
        var p = so as SpreadPatternSO;
        if (ctx.Spread == null)
        {
            yield return Wait(so.actionSeconds, ctx, stop);
            yield break;
        }

        // (선택) SO 수치를 Shooter에 주입
        ApplySpreadSO(ctx.Spread, p);

        if (useInternalShooterLoop)
        {
            // 스위치형: Shooter.Update가 fireRate에 맞춰 자동 발사
            ctx.Spread.isFire = true;
            yield return Wait(so.actionSeconds, ctx, stop);
            ctx.Spread.isFire = false;
        }
        else
        {
            // 외부 구동형: Runner가 직접 FireOnce 주기 호출
            ctx.Spread.isFire = false;
            float dur = Mathf.Max(0f, so.actionSeconds);
            float interval = (p.fireRate > 0f) ? 1f / p.fireRate : dur;
            float t = 0f, tShot = 0f;
            while (t < dur && !stop())
            {
                if (tShot <= 0f) { ctx.Spread.FireOnce(); tShot = interval; }
                float dt = ctx.DeltaTime();
                t += dt;
                tShot -= dt;
                yield return null;
            }
        }
    }

    static void ApplySpreadSO(BossPatternShooter s, SpreadPatternSO p)
    {
        if (!s || !p) return;
        s.totalBulletArrays = p.totalBulletArrays;
        s.bulletsPerArray = p.bulletsPerArray;
        s.spreadBetweenArrays = p.spreadBetweenArrays;
        s.spreadWithinArrays = p.spreadWithinArrays;
        s.startingAngle = p.startingAngle;

        s.spinRate = p.spinRate;
        s.spinModifier = p.spinModifier;
        s.invertSpin = p.invertSpin;
        s.maxSpinRate = p.maxSpinRate;
        s.autoInvertSpin = p.autoInvertSpin;
        // s.autoInvertSpinCycle 는 private이면 반영 생략 또는 리플렉션 사용

        s.fireRate = p.fireRate;
        s.fireOffset = p.fireOffset;

        s.bulletSpeed = p.bulletSpeed;
        s.bulletTTL = p.bulletTTL;
        s.bulletAcceleration = p.bulletAcceleration;
        s.bulletCurve = p.bulletCurve;
        s.bulletColor = p.bulletColor;
        s.spawnForwardOffset = p.spawnForwardOffset;
    }

    static IEnumerator Wait(float sec, BossRuntimeContext ctx, Func<bool> stop)
    {
        float t = 0f; float dur = Mathf.Max(0f, sec);
        while (t < dur && !stop()) { t += ctx.DeltaTime(); yield return null; }
    }
}
