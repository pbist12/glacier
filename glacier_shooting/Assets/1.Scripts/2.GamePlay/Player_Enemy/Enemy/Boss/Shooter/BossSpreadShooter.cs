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
    [Tooltip("전체 탄 배열(스포크/팔) 개수")]
    [Min(1)] public int totalBulletArrays = 3;

    [Tooltip("각 배열(팔) 안에서 발사되는 탄 수")]
    [Min(1)] public int bulletsPerArray = 3;

    [Tooltip("배열(팔) 사이의 각도 간격 (deg)")]
    public float spreadBetweenArrays = 120f;

    [Tooltip("각 배열 안에서의 탄 간 각도 간격 (deg)")]
    public float spreadWithinArrays = 1f;

    [Tooltip("시작 각도(전체 패턴에 더해짐)")]
    public float startingAngle = 0f;

    [Header("=== Spin ===")]
    [Tooltip("초당 회전 속도 (deg/s)")]
    public float spinRate = 0f;

    [Tooltip("초당 회전가속 (deg/s^2)")]
    public float spinModifier = 0f;

    [Tooltip("회전 방향 반전")]
    public bool invertSpin = false;

    [Tooltip("스핀 절대값 상한 (0 이하면 무제한)")]
    public float maxSpinRate = 360f;

    [Header("=== Firing ===")]
    [Tooltip("초당 발사 횟수 (0 이하면 정지)")]
    public float fireRate = 6f;

    [Tooltip("각 샷마다 X/Y 오프셋(로컬 공간, 회전에 따라 회전됨)")]
    public Vector2 fireOffset = Vector2.zero;

    [Header("=== Bullet Kinetics ===")]
    [Min(0.01f)] public float bulletSpeed = 8f;
    [Tooltip("탄의 수명(초)")]
    [Min(0.05f)] public float bulletTTL = 5f;
    [Tooltip("탄의 가속(속도 방향) m/s^2")]
    public float bulletAcceleration = 0f;
    [Tooltip("탄의 커브 강도(좌회전 +, 우회전 -) deg/s, 내부적으로 수직 가속으로 변환")]
    public float bulletCurve = 0f;

    [Header("=== Visual (optional) ===")]
    public Color bulletColor = Color.white;
    [Tooltip("스폰 시 앞쪽으로 조금 빼서 자기충돌 방지")]
    public float spawnForwardOffset = 0.2f;

    [Header("Control")]
    public bool isFire = true;

    // 내부 상태
    float _t;
    float _spin;     // 누적 스핀 각도
    float _spinVel;  // 현재 스핀 속도 (deg/s)

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

        // 스핀 적분
        float target = (invertSpin ? -Mathf.Abs(spinRate) : spinRate);
        _spinVel = Mathf.MoveTowards(_spinVel, target, Mathf.Abs(spinModifier) * Time.deltaTime);
        if (maxSpinRate > 0f) _spinVel = Mathf.Clamp(_spinVel, -Mathf.Abs(maxSpinRate), Mathf.Abs(maxSpinRate));
        _spin += _spinVel * Time.deltaTime;

        // 발사 간격
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

        // 로컬 오프셋(발사구 위치 미세조정) → 현재 회전에 맞게 회전
        Vector2 rotatedOffset = Rotate(fireOffset, rootAngle * Mathf.Deg2Rad);
        origin += rotatedOffset;

        // 각 배열(팔)
        for (int a = 0; a < totalBulletArrays; a++)
        {
            float arrayCenterDeg = rootAngle + a * spreadBetweenArrays;

            // 배열 내부의 탄들
            float half = (bulletsPerArray - 1) * 0.5f;
            for (int i = 0; i < bulletsPerArray; i++)
            {
                float idx = i - half;
                float deg = arrayCenterDeg + idx * spreadWithinArrays;
                float rad = deg * Mathf.Deg2Rad;

                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
                Vector2 spawnPos = origin + dir * spawnForwardOffset;

                // 스폰
                var go = hub.Spawn(poolKey, spawnPos, dir * bulletSpeed, bulletTTL, 1f, deg);

                // (선택) 색상
                if (go)
                {
                    var sr = go.GetComponentInChildren<SpriteRenderer>();
                    if (sr) sr.color = bulletColor;

                    // (선택) 가속/커브 적용용 보조 컴포넌트 부착
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
