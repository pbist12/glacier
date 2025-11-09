using UnityEngine;

namespace Test
{
    [DisallowMultipleComponent]
    public class Bullet : MonoBehaviour
    {
        [Header("Kinematics (degree, units/sec)")]
        public float directionDeg = 0f;      // 현재 진행 각도(도)
        public float speed = 3f;             // 속도 (unit/sec)
        public float acceleration = 0f;      // 가속 (unit/sec^2)
        public float curveDegPerSec = 0f;    // 매초 방향이 얼마나 휘는지(도/초)

        [Header("Lifetime")]
        public float ttlSeconds = 3f;

        [Header("Visual")]
        public SpriteRenderer sr;            // 작은 점 스프라이트(선택)
        public float pixelsPerUnit = 100f;

        float _life;
        Camera _cam;
        Vector3 _camMin, _camMax;

        public void Init(float directionDeg, float speed, float acceleration, float curveDegPerSec, float ttlSeconds)
        {
            this.directionDeg = directionDeg;
            this.speed = speed;
            this.acceleration = acceleration;
            this.curveDegPerSec = curveDegPerSec;
            this.ttlSeconds = ttlSeconds;
            _life = 0f;
            CacheCamBounds();
        }

        void Awake()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            CacheCamBounds();
        }

        void CacheCamBounds()
        {
            _cam = Camera.main;
            if (_cam == null) return;

            // 카메라 뷰 월드 경계
            var z = transform.position.z - _cam.transform.position.z;
            Vector3 bl = _cam.ViewportToWorldPoint(new Vector3(0, 0, z));
            Vector3 tr = _cam.ViewportToWorldPoint(new Vector3(1, 1, z));
            _camMin = bl;
            _camMax = tr;
            // 살짝 여유 경계
            float margin = 1f;
            _camMin -= Vector3.one * margin;
            _camMax += Vector3.one * margin;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _life += dt;
            if (_life >= ttlSeconds)
            {
                Destroy(gameObject);
                return;
            }

            // p5: direction += curve; speed += acceleration; (:contentReference[oaicite:2]{index=2})
            directionDeg += curveDegPerSec * dt;
            speed += acceleration * dt;

            // 진행 벡터
            float rad = directionDeg * Mathf.Deg2Rad;
            float dirX = Mathf.Cos(rad);
            float dirY = Mathf.Sin(rad) * -1f; // p5는 화면 y축이 아래(+), Unity는 위(+). p5의 부호를 맞춰주려면 -sin 사용(:contentReference[oaicite:3]{index=3})

            Vector3 v = new Vector3(dirX, dirY, 0f) * (speed * dt);
            transform.position += v;

            // 카메라 밖이면 제거 (p5의 화면 경계 제거 로직에 대응, Unity에 맞춰 일반화) (:contentReference[oaicite:4]{index=4})
            if (_cam != null)
            {
                var p = transform.position;
                if (p.x < _camMin.x || p.x > _camMax.x || p.y < _camMin.y || p.y > _camMax.y)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

}
