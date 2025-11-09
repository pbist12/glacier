using System;
using UnityEngine;

[Serializable]
public class NthAttackStyleAndBuffEffect : ItemEffect
{
    [Min(1)] public int nth = 4;

    [Header("Visual")]
    public bool tintNth = true;
    public Color tintColor = Color.red;

    [Header("Stat Multipliers (Nth shot only)")]
    [Tooltip("예: 0.5 = +50% damage on the Nth shot")]
    [Range(-0.9f, 5f)] public float damageBonusPercent = 0f;
    [Tooltip("예: 1.2 = +20% size")]
    [Range(0.1f, 5f)] public float sizeMul = 1f;
    [Range(0.1f, 5f)] public float speedMul = 1f;
    [Range(0.1f, 5f)] public float lifetimeMul = 1f;

    [Header("Optional FX on Nth shot")]
    public GameObject spawnVfx;         // 선택: N번째 발사 시 이펙트 스폰
    public AudioClip sfx;               // 선택: 사운드 재생
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private int _count;
    private ItemContext _ctx;

    public override void Apply(ItemContext ctx)
    {
        _ctx = ctx;
        _count = 0;
        GameEvents.BeforeBasicAttackFired += OnBeforeShot;   // 🔴 발사 직전 훅(B안)
    }

    public override void Remove(ItemContext ctx)
    {
        GameEvents.BeforeBasicAttackFired -= OnBeforeShot;
    }

    private void OnBeforeShot(ShotRequest req)
    {
        if ((PlayerShoot.Instance.nthAttack % nth) != 0) return;
        Debug.Log("이번 공격은" + PlayerShoot.Instance.nthAttack + "번째 공격입니다! 나눠서 남은 수:" + PlayerShoot.Instance.nthAttack % nth);

        // 1) 비주얼(색)
        if (tintNth) req.tint = tintColor;

        // 2) N번째 한 발에만 적용할 배수들
        if (Mathf.Abs(damageBonusPercent) > 0.0001f)
            req.damageMul *= (1f + damageBonusPercent);

        if (Mathf.Abs(sizeMul - 1f) > 0.0001f)
            req.sizeMul *= sizeMul;

        if (Mathf.Abs(speedMul - 1f) > 0.0001f)
            req.speedMul *= speedMul;

        if (Mathf.Abs(lifetimeMul - 1f) > 0.0001f)
            req.lifetimeMul *= lifetimeMul;

        // 3) 선택: 간단한 VFX/SFX (플레이어 위치 기준)
        if (_ctx != null && _ctx.owner != null)
        {
            if (spawnVfx != null)
                UnityEngine.Object.Instantiate(spawnVfx, _ctx.owner.transform.position, Quaternion.identity);

            if (sfx != null)
                AudioSource.PlayClipAtPoint(sfx, _ctx.owner.transform.position, sfxVolume);
        }
    }

    public override string Summary()
    {
        string dmg = Mathf.Abs(damageBonusPercent) > 0.0001f ? $" dmg {damageBonusPercent:+0%;-0%}" : "";
        string sz = Mathf.Abs(sizeMul - 1f) > 0.0001f ? $" size x{sizeMul:0.##}" : "";
        string spd = Mathf.Abs(speedMul - 1f) > 0.0001f ? $" speed x{speedMul:0.##}" : "";
        string life = Mathf.Abs(lifetimeMul - 1f) > 0.0001f ? $" life x{lifetimeMul:0.##}" : "";
        string col = tintNth ? $" tint #{ColorUtility.ToHtmlStringRGB(tintColor)}" : "";
        string fx = (spawnVfx || sfx) ? " +FX" : "";
        if (string.IsNullOrEmpty(dmg + sz + spd + life + col)) return $"Every {nth}th shot (no change){fx}";
        return $"Every {nth}th shot:{col}{dmg}{sz}{spd}{life}{fx}";
    }
}
