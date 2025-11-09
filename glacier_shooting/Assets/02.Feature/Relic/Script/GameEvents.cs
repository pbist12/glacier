using System;
using UnityEngine;

public class ShotRequest
{
    public Color? tint;          // 탄 색 (예: nth발사 빨강)
    public float damageMul = 1f; // 최종 데미지 배수
    public float speedMul = 1f; // 탄 속도 배수
    public float sizeMul = 1f; // 비주얼/콜라이더 스케일 배수
    public float lifetimeMul = 1f; // 수명 배수

    // 필요 시 확장:
    // public int addWays; 
    // public float homingChance;
}

public static class GameEvents
{
    // 🔹 발사 직전 훅(요청 객체를 전달해 수정하도록)
    public static event Action<ShotRequest> BeforeBasicAttackFired;

    // 🔹 기존 이벤트(그대로 유지)
    public static event Action SkillUsed;
    public static event Action BasicAttackFired;
    public static event Action BasicAttackHit;
    public static event Action Dashed;
    public static event Action Grazed;

    // ------- Raise helpers -------
    public static void RaiseBeforeBasicAttackFired(ShotRequest req)
        => BeforeBasicAttackFired?.Invoke(req);

    public static void RaiseSkillUsed() => SkillUsed?.Invoke();
    public static void RaiseBasicAttackFired() => BasicAttackFired?.Invoke();
    public static void RaiseBasicAttackHit() => BasicAttackHit?.Invoke();
    public static void RaiseDashed() => Dashed?.Invoke();
    public static void RaiseGrazed() => Grazed?.Invoke();
}
