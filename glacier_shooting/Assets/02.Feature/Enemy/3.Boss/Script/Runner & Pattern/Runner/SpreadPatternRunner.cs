// File: SpreadPatternRunner.cs
using Boss;
using Game.Data;
using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SpreadPatternRunner : MonoBehaviour, IPatternRunner, ISupportsPatternKind
{
    public bool useInternalShooterLoop = true; // 단순 on/off 방식
    public PatternKind Kind => PatternKind.Spread;

    public IEnumerator RunOnce(PatternSOBase so, BossRuntimeContext ctx, Func<bool> stop)
    {
        var p = so as SpreadPatternSO;
        var s = ctx.Spread; // BossPatternShooter (BulletSpawner 스타일 필드 사용)
        if (s == null)
        {
            yield return Wait(so.actionSeconds, ctx, stop);
            yield break;
        }

        // 1) SO -> Shooter 매핑 (BulletSpawner 스타일 변수명)
        ApplySpreadSO(s, p);

        // 2) Shooter 내부 Update 루프로 발사
        s.isFire = true;
        yield return Wait(so.actionSeconds, ctx, stop);
        s.isFire = false;
    }

    static void ApplySpreadSO(BossPatternShooter s, SpreadPatternSO p)
    {
        if (!s || !p) return;

        // === Arrays & Counts ===
        s.patternArrays = p.patternArrays; // SO의 totalBulletArrays -> Shooter의 patternArrays
        s.bulletsPerArray = p.bulletsPerArray;

        // === Angles (degree) ===
        s.spreadBetweenArray = p.spreadBetweenArray;
        s.spreadWithinArray = p.spreadWithinArray;
        s.startAngle = p.startAngle;
        // s.defaultAngle     // 필요하면 SO에 있을 때만 매핑

        // === Spin (deg/sec, deg/sec²) ===
        s.spinRate = p.spinRate;
        s.spinModificator = p.spinModificator;
        s.invertSpin = p.maxSpinRate > 0f;      // 옵션 사용한다면
        s.maxSpinRate = p.maxSpinRate;

        // === Fire Rate ===
        // SO의 fireRate를 "초당 발사 수"로 해석
        s.fireRatePerSec = Mathf.Max(0.01f, p.fireRatePerSec);

        // === Offsets ===
        s.xOffset = p.xOffset;
        s.yOffset = p.yOffset;

        // === Bullet Kinematics ===
        s.bulletSpeed = p.bulletSpeed;
        s.bulletAcceleration = p.bulletAcceleration;
        s.bulletCurveDegPerSec = p.bulletCurveDegPerSec;   // SO가 deg/sec 값이면 그대로
        s.bulletTTL = p.bulletTTL;

        // === Spawn ===
        // s.spawnForwardOffset = p.spawnForwardOffset;
        // s.fireOrigin         // 필요 시 ctx에서 넘겨 세팅
    }

    static IEnumerator Wait(float sec, BossRuntimeContext ctx, Func<bool> stop)
    {
        float t = 0f, dur = Mathf.Max(0f, sec);
        while (t < dur && !stop())
        {
            t += ctx.DeltaTime();
            yield return null;
        }
    }
}
