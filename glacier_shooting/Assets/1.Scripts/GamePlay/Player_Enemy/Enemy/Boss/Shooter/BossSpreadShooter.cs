using Unity.VisualScripting;
using UnityEngine;

public class BossPatternShooter : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private BulletPoolHub hub;
    [SerializeField] private BulletPoolKey poolKey = BulletPoolKey.Enemy;

    [Header("Fire Point (optional)")]
    [SerializeField] private Transform firePoint;

    [Header("=== Pattern Arrays ===")]
    [Tooltip("��ü ź �迭(����ũ/��) ����")]
    [Min(1)] public int totalBulletArrays = 3;

    [Tooltip("�� �迭(��) �ȿ��� �߻�Ǵ� ź ��")]
    [Min(1)] public int bulletsPerArray = 3;

    [Tooltip("�迭(��) ������ ���� ���� (deg)")]
    public float spreadBetweenArrays = 120f;

    [Tooltip("�� �迭 �ȿ����� ź �� ���� ���� (deg)")]
    public float spreadWithinArrays = 1f;

    [Tooltip("���� ����(��ü ���Ͽ� ������)")]
    public float startingAngle = 0f;

    [Header("=== Spin ===")]
    [Tooltip("�ʴ� ȸ�� �ӵ� (deg/s)")]
    public float spinRate = 0f;

    [Tooltip("�ʴ� ȸ������ (deg/s^2)")]
    public float spinModifier = 0f;

    [Tooltip("ȸ�� ���� ����")]
    public bool invertSpin = false;

    [Tooltip("���� ���밪 ���� (0 ���ϸ� ������)")]
    public float maxSpinRate = 360f;

    [Header("=== Firing ===")]
    [Tooltip("�ʴ� �߻� Ƚ�� (0 ���ϸ� ����)")]
    public float fireRate = 6f;

    [Tooltip("�� ������ X/Y ������(���� ����, ȸ���� ���� ȸ����)")]
    public Vector2 fireOffset = Vector2.zero;

    [Header("=== Bullet Kinetics ===")]
    [Min(0.01f)] public float bulletSpeed = 8f;
    [Tooltip("ź�� ����(��)")]
    [Min(0.05f)] public float bulletTTL = 5f;
    [Tooltip("ź�� ����(�ӵ� ����) m/s^2")]
    public float bulletAcceleration = 0f;
    [Tooltip("ź�� Ŀ�� ����(��ȸ�� +, ��ȸ�� -) deg/s, ���������� ���� �������� ��ȯ")]
    public float bulletCurve = 0f;

    [Header("=== Visual (optional) ===")]
    public Color bulletColor = Color.white;
    [Tooltip("���� �� �������� ���� ���� �ڱ��浹 ����")]
    public float spawnForwardOffset = 0.2f;

    [Header("Control")]
    public bool isFire = true;

    // ���� ����
    float _t;
    float _spin;     // ���� ���� ����
    float _spinVel;  // ���� ���� �ӵ� (deg/s)

    void Reset()
    {
        hub = FindAnyObjectByType<BulletPoolHub>();
    }

    void OnEnable()
    {
        _spinVel = (invertSpin ? -Mathf.Abs(spinRate) : spinRate);
    }

    void Update()
    {
        if (!isFire) return;
        if (!hub)
        {
            hub = FindAnyObjectByType<BulletPoolHub>();
            if (!hub) return;
        }

        // ���� ����
        float target = (invertSpin ? -Mathf.Abs(spinRate) : spinRate);
        _spinVel = Mathf.MoveTowards(_spinVel, target, Mathf.Abs(spinModifier) * Time.deltaTime);
        if (maxSpinRate > 0f) _spinVel = Mathf.Clamp(_spinVel, -Mathf.Abs(maxSpinRate), Mathf.Abs(maxSpinRate));
        _spin += _spinVel * Time.deltaTime;

        // �߻� ����
        if (fireRate <= 0f) return;
        _t += Time.deltaTime;
        float interval = 1f / fireRate;
        while (_t >= interval)
        {
            _t -= interval;
            FireOnce();
        }
    }

    void FireOnce()
    {
        Vector2 origin = firePoint ? (Vector2)firePoint.position : (Vector2)transform.position;
        float baseDeg = firePoint ? firePoint.eulerAngles.z : transform.eulerAngles.z;
        float rootAngle = baseDeg + startingAngle + _spin;

        // ���� ������(�߻籸 ��ġ �̼�����) �� ���� ȸ���� �°� ȸ��
        Vector2 rotatedOffset = Rotate(fireOffset, rootAngle * Mathf.Deg2Rad);
        origin += rotatedOffset;

        // �� �迭(��)
        for (int a = 0; a < totalBulletArrays; a++)
        {
            float arrayCenterDeg = rootAngle + a * spreadBetweenArrays;

            // �迭 ������ ź��
            float half = (bulletsPerArray - 1) * 0.5f;
            for (int i = 0; i < bulletsPerArray; i++)
            {
                float idx = i - half;
                float deg = arrayCenterDeg + idx * spreadWithinArrays;
                float rad = deg * Mathf.Deg2Rad;

                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
                Vector2 spawnPos = origin + dir * spawnForwardOffset;

                // ����
                var go = hub.Spawn(poolKey, spawnPos, dir * bulletSpeed, bulletTTL, 1f, deg);

                // (����) ����
                if (go)
                {
                    var sr = go.GetComponentInChildren<SpriteRenderer>();
                    if (sr) sr.color = bulletColor;

                    // (����) ����/Ŀ�� ����� ���� ������Ʈ ����
                    if (bulletAcceleration != 0f || bulletCurve != 0f)
                    {
                        //var kin = go.GetComponent<PatternBulletKinetics>();
                        //if (!kin) kin = go.AddComponent<PatternBulletKinetics>();
                        //kin.Set(bulletAcceleration, bulletCurve);
                    }
                }

#if UNITY_EDITOR
                Debug.DrawRay(spawnPos, dir * 1.2f, Color.green, 0.2f);
#endif
            }
        }
    }

    static Vector2 Rotate(in Vector2 v, float rad)
    {
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
