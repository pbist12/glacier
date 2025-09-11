using UnityEngine;

public class EnemyShooter_Linear : MonoBehaviour
{
    [Header("Pool / FirePoint")]
    [SerializeField] BulletPoolHub pool;
    [SerializeField] BulletPoolKey poolKey = BulletPoolKey.Player; // 인스펙터에서 Player/Enemy 선택
    public Transform firePoint;     // 없으면 transform.position 사용

    [Header("Bullet")]
    public float bulletSpeed = 7f;
    public float bulletLifetime = 5f;

    [Header("Fire")]
    public float fireInterval = 0.45f;         // 발사 간격(초)
    [Range(-180f, 180f)] public float fireAngleDeg = -90f; // -90=아래
    public float firstShotDelayFactor = 0.5f;  // 첫 발만 살짝 빠르게(0~1)

    float _nextFireAt;

    void OnEnable()
    {
        // 시작 템포 살짝 가볍게
        pool = GameObject.FindFirstObjectByType<BulletPoolHub>();
        _nextFireAt = Time.time + fireInterval * Mathf.Clamp01(firstShotDelayFactor);
    }

    void Update()
    {
        if (pool == null) return; // 필수
        if (Time.time < _nextFireAt) return;
        _nextFireAt = Time.time + fireInterval;

        Vector2 origin = firePoint ? (Vector2)firePoint.position : (Vector2)transform.position;
        float rad = fireAngleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        // 네 BulletPool 시그니처에 정확히 맞춤
        pool.Spawn(poolKey, origin, dir * bulletSpeed, bulletLifetime,1f, fireAngleDeg);
    }

    // 씬에서 발사 방향 미리보기
    void OnDrawGizmosSelected()
    {
        Vector3 fp = firePoint ? firePoint.position : transform.position;
        var dir = new Vector2(Mathf.Cos(fireAngleDeg * Mathf.Deg2Rad), Mathf.Sin(fireAngleDeg * Mathf.Deg2Rad));
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.85f);
        Gizmos.DrawLine(fp, fp + (Vector3)(dir.normalized * 1.5f));
    }
}
