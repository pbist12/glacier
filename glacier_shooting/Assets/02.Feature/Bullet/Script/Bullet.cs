// File: Bullet.cs
using UnityEngine;

[DisallowMultipleComponent]
public class Bullet : MonoBehaviour, IBulletKinetics, IPooledBulletReset
{
    #region 변수
    // =============================
    //  Pooling
    // =============================
    [HideInInspector] public BulletPoolHub hub;
    [HideInInspector] public BulletPoolKey poolKey;

    // =============================
    //  Common
    // =============================
    [Header("Common")]
    public float damage;
    public Vector2 velocity;
    public float lifetime = 4f;
    public bool despawnOnBorderExit = true;

    // =============================
    //  Homing
    // =============================
    [Header("Homing")]
    public bool homingEnabled = false;
    public float speed = 5f;        // Facing 기반 전진 속도
    public float rotateSpeed = 200f;
    public float homingDelay = 0.6f;
    public float homingDuration = 1.8f; // 0 이하면 무제한
    public string targetTag = "Player";

    // =============================
    //  Debug / Collision
    // =============================
    [Header("Collision Debug")]
    public float hitRadius = 0.12f;
    public float defHtiRadius; // 원 변수명 유지(오타 그대로)
    [Tooltip("씬 뷰에서 판정 반경을 Gizmo로 표시할지 여부")]
    public bool drawHitRadiusGizmo = true;

    [HideInInspector] public Vector2 prevPos;

    // =============================
    //  Kinetics (Injected)
    // =============================
    [Header("Kinetics (Injected)")]
    [Tooltip("직선 비유도 이동 시 초당 가속도 (unit/sec^2)")]
    public float accel = 0f;                 // Launch/Init에서 주입
    [Tooltip("직선 비유도 이동 시 초당 커브(각속도) (deg/sec)")]
    public float curveDegPerSec = 0f;        // Launch/Init에서 주입

    // =============================
    //  Internal
    // =============================
    float _age;
    float _phaseTimer;
    Transform _tf;
    Transform _target;

    enum State { StraightByVelocity, PreHoming, Homing, StraightFacing }
    State _state = State.StraightByVelocity;
    #endregion

    // =========================================================
    //  Unity lifecycle
    // =========================================================
    void Awake()
    {
        _tf = transform;

        // defHtiRadius가 미설정(0 이하)이면 현재 hitRadius를 기본값으로 백업
        if (defHtiRadius <= 0f) defHtiRadius = hitRadius;
        hitRadius = defHtiRadius;
    }

    void OnEnable()
    {
        // 풀에서 되살아날 때 안전 초기화 (허브 OnBeforeSpawnFromPool 이후)
        _age = 0f;
        _phaseTimer = 0f;
        hitRadius = defHtiRadius;

        // 이전 프레임 궤적 잔존 방지
        prevPos = _tf.position;

        if (homingEnabled)
        {
            TryAcquireTargetIfNull();
            _state = State.PreHoming;

            // 호밍 전진은 speed 사용 → 미지정이면 velocity 기반으로 보정
            if (speed <= 0f) speed = velocity.magnitude;
        }
        else
        {
            _state = State.StraightByVelocity;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 수명
        _age += dt;
        if (_age >= lifetime) { Despawn(); return; }

        // 프레임 위치 저장 & 페이즈 타이머 업데이트
        prevPos = _tf.position;
        _phaseTimer += dt;

        if (!homingEnabled)
        {
            // === 비유도: velocity 기반 직진 + 가속/커브 ===
            ApplyStraightKinetics(dt);
            _tf.position += (Vector3)(velocity * dt);
            return;
        }

        // === 유도 모드 ===
        switch (_state)
        {
            case State.PreHoming:
                MoveForwardFacing(dt);
                if (_phaseTimer >= homingDelay)
                {
                    _phaseTimer = 0f;
                    _state = (_target != null) ? State.Homing : State.StraightFacing;
                }
                break;

            case State.Homing:
                if (_target == null)
                {
                    _state = State.StraightFacing;
                    MoveForwardFacing(dt);
                    break;
                }

                FaceTowards(_target.position, rotateSpeed, dt);
                MoveForwardFacing(dt);

                if (homingDuration > 0f && _phaseTimer >= homingDuration)
                {
                    _state = State.StraightFacing;
                }
                break;

            case State.StraightFacing:
                MoveForwardFacing(dt);
                break;

            case State.StraightByVelocity:
                _tf.position += (Vector3)(velocity * dt);
                break;
        }
    }

    void OnBecameInvisible()
    {
        if (despawnOnBorderExit && gameObject.activeSelf)
            Despawn();
    }

    void OnDrawGizmos()
    {
        if (!drawHitRadiusGizmo) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }

    // =========================================================
    //  Public API
    // =========================================================
    public void Despawn()
    {
        if (!gameObject.activeSelf) return;
        if (hub != null) hub.Despawn(this);
        else gameObject.SetActive(false); // 풀 미연결 상황에서도 안전 반납
    }

    /// <summary>
    /// 풀/허브에서 한 번에 주입하는 초기화 경로.
    /// </summary>
    public void Launch(Vector2 dir, float speed_, float accel_, float curve_, float ttlSeconds)
    {
        lifetime = ttlSeconds;
        _age = 0f;

        // Launch는 항상 비유도로 시작(기존 동작 유지)
        homingEnabled = false;
        _state = State.StraightByVelocity;

        Vector2 d = (dir.sqrMagnitude > 0.0001f) ? dir.normalized : (Vector2)(-_tf.up);
        velocity = d * Mathf.Max(0f, speed_);
        accel = accel_;
        curveDegPerSec = curve_;

        prevPos = _tf.position;
    }

    /// <summary>
    /// 풀에서 꺼내 활성화되기 직전 호출(허브가 호출).
    /// TrailRenderer/Particle/Collider 초기화 등 상태 리셋.
    /// </summary>
    public void OnBeforeSpawnFromPool()
    {
        var trail = GetComponent<TrailRenderer>();
        if (trail) trail.Clear();

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d) { rb2d.linearVelocity = Vector2.zero; rb2d.angularVelocity = 0f; }

        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        var col2d = GetComponent<Collider2D>();
        if (col2d) col2d.enabled = true;

        var col3d = GetComponent<Collider>();
        if (col3d) col3d.enabled = true;

        _age = 0f;
        _phaseTimer = 0f;
        hitRadius = defHtiRadius;
        prevPos = _tf.position;
    }

    /// <summary>
    /// 풀로 반납되기 직전 호출(허브가 호출).
    /// </summary>
    public void OnAfterDespawnToPool()
    {
        var trail = GetComponent<TrailRenderer>();
        if (trail) trail.Clear();

        var col2d = GetComponent<Collider2D>();
        if (col2d) col2d.enabled = false;

        var col3d = GetComponent<Collider>();
        if (col3d) col3d.enabled = false;
    }

    /// <summary>
    /// 예전 BulletSpawner 호환: 각도/속도/가속/커브/TTL로 초기화.
    /// </summary>
    public void Init(float directionDeg, float speed_, float acceleration_, float curveDegPerSec_, float ttlSeconds)
    {
        // 방향각을 회전으로 반영(총구 -up이 진행방향)
        _tf.rotation = Quaternion.Euler(0f, 0f, directionDeg);

        // 각도 → 방향 벡터 변환 후 Launch로 위임
        float r = directionDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(r), Mathf.Sin(r));

        Launch(dir, speed_, acceleration_, curveDegPerSec_, ttlSeconds);
    }

    /// <summary>
    /// 예전 BulletSpawner 호환(데미지 포함 버전).
    /// </summary>
    public void Init(float directionDeg, float speed_, float acceleration_, float curveDegPerSec_, float ttlSeconds, float damage_)
    {
        damage = damage_;
        Init(directionDeg, speed_, acceleration_, curveDegPerSec_, ttlSeconds);
    }

    /// <summary>
    /// 유도 On/Off 토글(대상 지정 옵션)
    /// </summary>
    public void SetHomingEnabled(bool on, Transform targetOverride = null)
    {
        homingEnabled = on;
        if (targetOverride) _target = targetOverride;

        _phaseTimer = 0f;

        if (homingEnabled)
        {
            TryAcquireTargetIfNull();
            if (speed <= 0f) speed = velocity.magnitude;
            _state = State.PreHoming;
        }
        else
        {
            _state = State.StraightByVelocity;
        }
    }

    public void SetTarget(Transform t) => _target = t;

    public void UpdateHitRadius(float mul)
    {
        // 기존 누적 곱 동작 유지
        hitRadius = hitRadius * mul;
    }

    // =========================================================
    //  Movement helpers
    // =========================================================
    void MoveForwardFacing(float dt)
    {
        _tf.position += -_tf.up * speed * dt; // 스프라이트 -Up이 진행방향
    }

    void ApplyStraightKinetics(float dt)
    {
        // 커브
        if (curveDegPerSec != 0f)
            velocity = Rotate(velocity, curveDegPerSec * dt);

        // 가속
        if (accel != 0f)
        {
            float vMag = velocity.magnitude;
            if (vMag > 0f)
            {
                Vector2 dir = velocity / vMag;
                vMag = Mathf.Max(0f, vMag + accel * dt);
                velocity = dir * vMag;
            }
            else
            {
                // 정지 상태면 forward(로컬 -up) 기준으로 속도 부여
                velocity = (Vector2)(-_tf.up) * Mathf.Max(0f, accel * dt);
            }
        }
    }

    void FaceTowards(in Vector3 worldPos, float degPerSec, float dt)
    {
        Vector2 dir = (worldPos - _tf.position).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f; // -up 진행 오프셋
        float newAngle = Mathf.MoveTowardsAngle(_tf.eulerAngles.z, targetAngle, degPerSec * dt);
        _tf.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    static Vector2 Rotate(in Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    void TryAcquireTargetIfNull()
    {
        if (_target != null) return;
        var go = GameObject.FindGameObjectWithTag(targetTag);
        if (go) _target = go.transform;
    }
}
