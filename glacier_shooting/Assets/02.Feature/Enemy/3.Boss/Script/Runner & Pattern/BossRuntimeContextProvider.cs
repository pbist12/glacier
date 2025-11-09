using UnityEngine;

namespace Boss
{
    [DisallowMultipleComponent]
    public class BossRuntimeContextProvider : MonoBehaviour
    {
        [Header("Refs")]
        public Transform boss;
        public BulletPoolHub bullets;
        public BossPatternShooter spread;
        public BossLaserShooter laser;
        public BossMover mover;

        [Header("Time")]
        public bool useUnscaledTime = false;

        private void OnEnable()
        {
            boss = GetComponent<Transform>();
            bullets = FindFirstObjectByType<BulletPoolHub>();
            spread = GetComponentInChildren<BossPatternShooter>();
            laser = GetComponentInChildren<BossLaserShooter>();
            mover = GetComponentInChildren<BossMover>();
        }

        public BossRuntimeContext Build()
        {
            return new BossRuntimeContext
            {
                Boss = boss ? boss : transform,
                Bullets = bullets,
                Spread = spread,
                Laser = laser,
                Mover = mover,
                DeltaTime = useUnscaledTime
                    ? (System.Func<float>)(() => Time.unscaledDeltaTime)
                    : (() => Time.deltaTime),
            };
        }
    }
}
