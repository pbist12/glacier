// File: SpreadPatternSO.cs
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Monster/Boss/Pattern/Spread", fileName = "SpreadPatternSO")]
    public class SpreadPatternSO : PatternSOBase
    {
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
        public float xOffset = 0f;
        public float yOffset = 0f;

        [Header("Bullet Kinematics")]
        public float bulletSpeed = 3f;           // unit/sec
        public float bulletAcceleration = 0f;    // unit/sec^2
        public float bulletCurveDegPerSec = 0f;  // 도/초 (탄 구현체 해석에 따름)
        public float bulletTTL = 3f;             // 초

#if UNITY_EDITOR
        private void OnValidate()
        {
            kind = PatternKind.Spread;
            patternArrays = Mathf.Max(1, patternArrays);
            bulletsPerArray = Mathf.Max(1, bulletsPerArray);
            if (maxSpinRate < 0f) maxSpinRate = 0f;
            if (fireRatePerSec < 0f) fireRatePerSec = 0;
            bulletSpeed = Mathf.Max(0.01f, bulletSpeed);
            bulletTTL = Mathf.Max(0.05f, bulletTTL);
            telegraphSeconds = Mathf.Max(0f, telegraphSeconds);
            postDelaySeconds = Mathf.Max(0f, postDelaySeconds);
        }
#endif
    }

}
