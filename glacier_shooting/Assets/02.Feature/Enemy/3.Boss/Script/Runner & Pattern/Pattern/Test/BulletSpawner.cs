using UnityEngine;

namespace Test
{
    public class BulletSpawner : MonoBehaviour
    {
        public bool isFire;

        [Header("Prefab")]
        public Bullet bulletPrefab;

        [Header("Arrays & Counts")]
        public int patternArrays = 2;        // 총알 배열(링) 수
        public int bulletsPerArray = 10;     // 배열당 총알 수

        [Header("Angles (degree)")]
        public float spreadBetweenArray = 180f;  // 배열 간 기준각 간격
        public float spreadWithinArray = 90f;    // 배열 내부에서 첫~마지막 총알이 벌어지는 각도 총량
        public float startAngle = 0f;            // 최초 기준 각
        public float defaultAngle = 0f;          // 필요 시 추가 기준각

        [Header("Spin (degree/sec)")]
        public float spinRate = 0f;          // 현재 스핀 속도
        public float spinModificator = 0f;   // 스핀 가감속(초당)
        public bool invertSpin = true;       // 최대치 도달 시 반전?
        public float maxSpinRate = 10f;      // 스핀 속도 최대치(절댓값)

        [Header("Fire Rate")]
        public float fireRatePerSec = 5f;    // 초당 발사 횟수

        [Header("Offsets (world units)")]
        public float xOffset = 0f;
        public float yOffset = 0f;
        public float objectWidth = 0f;       // p5 변수 보존용
        public float objectHeight = 0f;      // p5 변수 보존용

        [Header("Bullet Kinematics")]
        public float bulletSpeed = 3f;           // unit/sec
        public float bulletAcceleration = 0f;    // unit/sec^2
        public float bulletCurveDegPerSec = 0f;  // 도/초
        public float bulletTTL = 3f;             // 초

        [Header("Misc")]
        public Transform fireOrigin;             // 없으면 spawner transform 사용

        // --- 내부 상태 ---
        [SerializeField] float _spinCurrent;
        float _acc;                 // 발사용 누적 시간
        float _interval;            // 1 / fireRatePerSec
        const int MaxShotsPerFrame = 500; // 프레임 드랍 시 폭주 방지

        void OnEnable()
        {
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

            _spinCurrent += spinModificator * dt;

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
            if (bulletPrefab == null) return;
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
            Bullet b = Instantiate(bulletPrefab, pos, Quaternion.identity);
            b.Init(directionDeg, bulletSpeed, bulletAcceleration, bulletCurveDegPerSec, bulletTTL);
        }
    }
}
