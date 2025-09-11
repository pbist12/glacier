using UnityEngine;

public class EnemyShooter_Linear : MonoBehaviour
{
    [Header("Pool / FirePoint")]
    [SerializeField] BulletPoolHub pool;
    [SerializeField] BulletPoolKey poolKey = BulletPoolKey.Player; // �ν����Ϳ��� Player/Enemy ����
    public Transform firePoint;     // ������ transform.position ���

    [Header("Bullet")]
    public float bulletSpeed = 7f;
    public float bulletLifetime = 5f;

    [Header("Fire")]
    public float fireInterval = 0.45f;         // �߻� ����(��)
    [Range(-180f, 180f)] public float fireAngleDeg = -90f; // -90=�Ʒ�
    public float firstShotDelayFactor = 0.5f;  // ù �߸� ��¦ ������(0~1)

    float _nextFireAt;

    void OnEnable()
    {
        // ���� ���� ��¦ ������
        pool = GameObject.FindFirstObjectByType<BulletPoolHub>();
        _nextFireAt = Time.time + fireInterval * Mathf.Clamp01(firstShotDelayFactor);
    }

    void Update()
    {
        if (pool == null) return; // �ʼ�
        if (Time.time < _nextFireAt) return;
        _nextFireAt = Time.time + fireInterval;

        Vector2 origin = firePoint ? (Vector2)firePoint.position : (Vector2)transform.position;
        float rad = fireAngleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        // �� BulletPool �ñ״�ó�� ��Ȯ�� ����
        pool.Spawn(poolKey, origin, dir * bulletSpeed, bulletLifetime,1f, fireAngleDeg);
    }

    // ������ �߻� ���� �̸�����
    void OnDrawGizmosSelected()
    {
        Vector3 fp = firePoint ? firePoint.position : transform.position;
        var dir = new Vector2(Mathf.Cos(fireAngleDeg * Mathf.Deg2Rad), Mathf.Sin(fireAngleDeg * Mathf.Deg2Rad));
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.85f);
        Gizmos.DrawLine(fp, fp + (Vector3)(dir.normalized * 1.5f));
    }
}
