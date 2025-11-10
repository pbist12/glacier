using UnityEngine;

public class BossPatternShooter : MonoBehaviour
{
    [Header("On/Off")]
    public bool isFire = true;

    [Header("Pool (Required)")]
    [SerializeField] private BulletPoolHub hub;              // 풀 허브
    [SerializeField] private BulletPoolKey poolKey = BulletPoolKey.Enemy;

    [Header("Arrays & Counts")]
    public int patternArrays = 2;        // 고리(링) 수
    public int bulletsPerArray = 10;     // 배열당 총알 수

    [Header("Angles (degree)")]
    public float spreadBetweenArray = 180f;  // 배열 간 기준각 간격
    public float spreadWithinArray = 90f;    // 배열 내부에서 첫~마지막 총알 벌어짐
    public float startAngle = 0f;            // 배열 기준 시작각
    public float defaultAngle = 0f;          // 누적 기준각(회전이 여기에 더해짐)

    [Header("Spin (deg/sec, deg/sec²)")]
    public float spinRate = 0f;          // 초당 회전(도/초)
    public float spinModificator = 0f;   // 초당 가감속(도/초²)
    public bool invertSpin = true;       // 상한 도달 시 가감속 방향 반전?
    public float maxSpinRate = 360f;     // |spinRate| 상한(도/초)

    [Header("Fire Rate")]
    public float fireRatePerSec = 5f;    // 초당 발사 횟수 (<=0이면 0.2초 간격으로 간주)

    [Header("Offsets (world units)")]
    public Transform fireOrigin;         // 없으면 transform.position
    public float xOffset = 0f;
    public float yOffset = 0f;

    [Header("Bullet Kinematics")]
    public float bulletSpeed = 3f;           // unit/sec
    public float bulletAcceleration = 0f;    // unit/sec^2
    public float bulletCurveDegPerSec = 0f;  // 도/초 (탄 구현체 해석에 따름)
    public float bulletTTL = 3f;             // 초

    // 내부 상태
    [SerializeField] float _spinCurrent;
    float _acc;                 // 발사용 누적 시간
    float _interval;            // 1 / fireRatePerSec
    const int MaxShotsPerFrame = 500; // 프레임 드랍 시 폭주 방지

    void OnEnable()
    {
        if (!hub) hub = FindAnyObjectByType<BulletPoolHub>();
        _spinCurrent = spinRate;
        _acc = 0f;
        _interval = fireRatePerSec > 0f ? 1f / fireRatePerSec : 0.2f;
    }

    void OnDisable()
    {
        _acc = 0f;
    }

    void Update()
    {
        if (!isFire) return;

        // 1) 실제 경과시간 기반 스핀 업데이트
        float dt = Time.deltaTime;

        // (옵션) 큰 프레임 스파이크 완화
        if (dt > 0.1f) dt = 0.1f;

        _spinCurrent = spinModificator * dt;

        if (invertSpin && Mathf.Abs(_spinCurrent) >= Mathf.Abs(maxSpinRate))
        {
            _spinCurrent = Mathf.Sign(_spinCurrent) * Mathf.Abs(maxSpinRate);
            spinModificator = -spinModificator; // 가감속 방향 반전
        }

        startAngle += _spinCurrent * dt;
        if (startAngle > 360f || startAngle < -360f) startAngle %= 360f;

        // 2) 발사 템포: 프레임당 여러 발 허용
        _interval = fireRatePerSec > 0f ? 1f / fireRatePerSec : 0.2f; // 런타임 즉시 반영
        _acc += dt;

        int safety = 0;
        while (_acc >= _interval && safety < MaxShotsPerFrame)
        {
            SpawnPattern();
            _acc -= _interval;
            safety++;
        }
    }

    void SpawnPattern()
    {
        if (patternArrays < 1 || bulletsPerArray < 1) return;

        Vector3 origin = fireOrigin ? fireOrigin.position : transform.position;
        origin += new Vector3(xOffset, yOffset, 0f);

        // 각 배열(링)별 기준각
        for (int a = 0; a < patternArrays; a++)
        {
            float baseAngle = startAngle + (a * spreadBetweenArray);

            // 배열 내부 총알 간 간격
            float step = (bulletsPerArray > 1) ? (spreadWithinArray / (bulletsPerArray - 1)) : 0f;
            float start = baseAngle - spreadWithinArray * 0.5f;

            for (int i = 0; i < bulletsPerArray; i++)
            {
                float angleDeg = start + (step * i) + defaultAngle;
                SpawnBullet(origin, angleDeg);
            }
        }
    }

    void SpawnBullet(Vector3 pos, float directionDeg)
    {
        float r = directionDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        Vector2 vel = dir * bulletSpeed;

        if (hub)
        {
            Bullet go = hub.Spawn(poolKey, pos, vel, bulletTTL, 1f, directionDeg);
            if (go)
            {
                if (go.TryGetComponent<Bullet>(out var bullet))
                {
                    bullet.Init(directionDeg, bulletSpeed, bulletAcceleration, bulletCurveDegPerSec, bulletTTL);
                }
                // 2) IBulletKinetics 형태도 대응
                else if (go.TryGetComponent<IBulletKinetics>(out var kin))
                {
                    kin.Launch(dir, bulletSpeed, bulletAcceleration, bulletCurveDegPerSec, bulletTTL);
                }
                return;
            }
        }
    }
}

public interface IBulletKinetics
{
    void Launch(Vector2 dir, float speed, float accel, float curve, float ttlSeconds);
}
