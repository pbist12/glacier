using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class ProcOnEventEffect : ItemEffect
{
    public enum SimpleTrigger { Manual, OnHit, OnGraze, OnDash }
    public SimpleTrigger trigger = SimpleTrigger.Manual;

    [Range(0f, 1f)] public float chance = 0.2f;
    [Tooltip("내부 쿨다운(초)")] public float icd = 0.5f;

    [Header("액션 (데모)")]
    public bool crit2x = false;          // 잠깐 +100% 피해
    public bool spawnExplosion = false;   // 착탄 폭발 프리팹 스폰(여긴 함수만 남겨둠)

    private ItemContext _ctx;
    private BuffHost _host;
    private float _lastProcTime = -999f;

    public override void Apply(ItemContext ctx)
    {
        _ctx = ctx;
        if (_ctx != null && _ctx.owner != null)
        {
            _host = _ctx.owner.GetComponent<BuffHost>();
            if (_host == null) _host = _ctx.owner.AddComponent<BuffHost>();
        }
    }

    public override void Remove(ItemContext ctx)
    {
        // 구독형 이벤트를 쓰지 않았으므로 정리 없음
    }

    public void ManualTryProc()
    {
        TryProc();
    }

    /// Projectile/Player 등에서 상황 맞게 호출(예: 명중/그레이즈/대시 순간)
    public void OnHit() { if (trigger == SimpleTrigger.OnHit) TryProc(); }
    public void OnGraze() { if (trigger == SimpleTrigger.OnGraze) TryProc(); }
    public void OnDash() { if (trigger == SimpleTrigger.OnDash) TryProc(); }

    private void TryProc()
    {
        if (Time.time - _lastProcTime < icd) return;
        if (UnityEngine.Random.value > chance) return;
        _lastProcTime = Time.time;

        if (crit2x && _ctx != null && _ctx.stats != null)
        {
            _ctx.stats.AddModifier(StatType.BulletDamage, 0, 1.0f); // +100%
            if (_host != null) _host.StartCoroutine(RevertCritSoon());
            else _ctx.stats.AddModifier(StatType.BulletDamage, 0, -1.0f);
        }

        if (spawnExplosion)
        {
            SpawnExplosionAtPlayer();
        }
    }

    private IEnumerator RevertCritSoon()
    {
        yield return new WaitForSeconds(0.05f);
        if (_ctx != null && _ctx.stats != null)
            _ctx.stats.AddModifier(StatType.BulletDamage, 0, -1.0f);
    }

    private void SpawnExplosionAtPlayer()
    {
        // TODO: 폭발 프리팹 스폰 지점/연출 연결
        // var go = GameObject.Instantiate(explosionPrefab, _ctx.owner.transform.position, Quaternion.identity);
    }

    public override string Summary()
    {
        return trigger.ToString() + ": " + (chance * 100f).ToString("0.#") + "% proc"
             + (crit2x ? " (crit x2)" : "")
             + (spawnExplosion ? " (explosion)" : "");
    }
}
