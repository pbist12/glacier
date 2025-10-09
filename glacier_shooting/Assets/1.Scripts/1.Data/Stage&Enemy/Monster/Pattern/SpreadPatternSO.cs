// File: SpreadPatternSO.cs
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Boss/Pattern/Spread", fileName = "SpreadPatternSO")]
    public class SpreadPatternSO : PatternSOBase
    {
        [Header("=== Pattern Arrays ===")]
        [Min(1)] public int totalBulletArrays = 3;     // ��ü �� ����
        [Min(1)] public int bulletsPerArray = 3;       // �ȴ� ź ��
        public float spreadBetweenArrays = 120f;       // �� ���� ����
        public float spreadWithinArrays = 1f;          // �� ���� ����
        public float startingAngle = 0f;               // ���� ����(��ü ������)

        [Header("=== Spin ===")]
        public float spinRate = 0f;                    // deg/s
        public float spinModifier = 0f;                // deg/s^2
        public bool invertSpin = false;
        public float maxSpinRate = 360f;               // 0 ���ϸ� ������
        public bool autoInvertSpin = false;
        public float autoInvertSpinCycle = 0f;         // ��

        [Header("=== Firing ===")]
        public float fireRate = 6f;                    // shots per second (0 ���ϸ� ����)
        public Vector2 fireOffset = Vector2.zero;      // ���� ������

        [Header("=== Bullet Kinetics ===")]
        public BulletPoolKey poolKey = BulletPoolKey.Enemy;
        [Min(0.01f)] public float bulletSpeed = 8f;
        [Min(0.05f)] public float bulletTTL = 5f;
        public float bulletAcceleration = 0f;          // m/s^2 (���� ����)
        public float bulletCurve = 0f;                 // deg/s (Ŀ��)

        [Header("=== Visual ===")]
        public Color bulletColor = Color.white;
        public float spawnForwardOffset = 0.2f;        // �ڱ��浹 ���� ���� ������

#if UNITY_EDITOR
        private void OnValidate()
        {
            kind = PatternKind.Spread;
            totalBulletArrays = Mathf.Max(1, totalBulletArrays);
            bulletsPerArray = Mathf.Max(1, bulletsPerArray);
            if (maxSpinRate < 0f) maxSpinRate = 0f;
            if (autoInvertSpin && autoInvertSpinCycle < 0f) autoInvertSpinCycle = 0f;
            if (fireRate < 0f) fireRate = 0f;
            bulletSpeed = Mathf.Max(0.01f, bulletSpeed);
            bulletTTL = Mathf.Max(0.05f, bulletTTL);
            spawnForwardOffset = Mathf.Max(0f, spawnForwardOffset);
            telegraphSeconds = Mathf.Max(0f, telegraphSeconds);
            postDelaySeconds = Mathf.Max(0f, postDelaySeconds);
        }
#endif
    }

}
