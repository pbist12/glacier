// File: BossSpreadShooter.cs
using UnityEngine;

[DisallowMultipleComponent]
public class BossSpreadShooter : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private BulletPoolHub hub;
    [SerializeField] private BulletPoolKey poolKey = BulletPoolKey.Enemy;

    [Header("Fire Point")]
    [SerializeField] private Transform firePoint;

    [Header("Bullet")]
    [Min(0.1f)] public float bulletSpeed = 8f;
    [Min(0.05f)] public float bulletLifetime = 5f;

    [Header("Fire")]
    public bool isFire;

    [Header("Spread Params")]
    [Min(1)] public int spreadBulletCount = 9;
    public float spreadAngleStep = 8f;
    public float spreadFireInterval = 0.15f;
    [Min(1)] public int spreadBurstCount = 6;
    public float spreadBurstCoolDownTime = 1.0f;
    public float spreadRotationStep = 6f;

    // 내부 상태
    [SerializeField] private float _spreadOffsetAngle = 0f;
    private float _burstTimer = 0f;
    private float _cooldownTimer = 0f;
    private int _burstShots = 0;
    private bool _inCooldown = false;

    void Reset()
    {
        hub = FindAnyObjectByType<BulletPoolHub>();
    }

    void Update()
    {
        if (!enabled) return;
        if (!hub) { hub = FindAnyObjectByType<BulletPoolHub>(); if (!hub) return; }
        if (!isFire) return;

        if (!_inCooldown)
        {
            _burstTimer += Time.deltaTime;
            if (_burstTimer >= spreadFireInterval)
            {
                _burstTimer = 0f;
                FireSpreadOnce(); // 한 번 쏘고
                _spreadOffsetAngle += spreadRotationStep;
                _burstShots++;

                if (_burstShots >= spreadBurstCount)
                {
                    _inCooldown = true;
                    _burstShots = 0;
                    _cooldownTimer = 0f;
                }
            }
        }
        else
        {
            _cooldownTimer += Time.deltaTime;
            if (_cooldownTimer >= spreadBurstCoolDownTime)
            {
                _inCooldown = false;
                _cooldownTimer = 0f;
                _burstTimer = 0f;
                // 필요 시 회전 누적 초기화:
                // _spreadOffsetAngle = 0f;
            }
        }
    }

    public void FireSpreadOnce()
    {
        if (!hub) return;

        Vector2 origin = firePoint ? (Vector2)firePoint.position : (Vector2)transform.position;
        float baseDeg = firePoint ? firePoint.eulerAngles.z : transform.eulerAngles.z;

        float half = (spreadBulletCount - 1) * 0.5f;
        const float spawnForwardOffset = 0.2f; // 자기충돌 회피

        for (int i = 0; i < spreadBulletCount; i++)
        {
            float idx = i - half;
            float deg = baseDeg + _spreadOffsetAngle + idx * spreadAngleStep;
            float rad = deg * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            Vector2 spawnPos = origin + dir * spawnForwardOffset;

            hub.Spawn(poolKey, spawnPos, dir * bulletSpeed, bulletLifetime,1f, deg);
#if UNITY_EDITOR
            Debug.DrawRay(spawnPos, dir * 1.2f, Color.green, 0.2f);
#endif
        }
    }
}
