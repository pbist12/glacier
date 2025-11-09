using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Move_SeekPlayer : EnemyMoveBase
{
    [Header("Target Acquire")]
    public bool autoFindPlayer = true;
    public string targetTag = "Player";
    public bool refreshByTag = true;
    [Min(0.05f)] public float refreshInterval = 0.25f;
    public bool reacquireOnNull = true;
    public float maxChaseDistance = 0f; // 0 = unlimited
    public Transform explicitTarget;

    [Header("Steering (Physics)")]
    public float maxSpeed = 6f;
    public float maxAccel = 20f;
    public float slowRadius = 2.0f;
    public float arriveStopDistance = 0.35f;
    public ForceMode2D forceMode = ForceMode2D.Force;

    [Header("Aim")]
    public Vector2 aimPointOffset = Vector2.zero;
    [Min(0f)] public float targetSmooth = 10f;

    [Header("Lost Target Handling")]
    public float lostPursuitTimeout = 2.0f;

    // ▼▼▼ 추가: 호밍 시간 제한 옵션 ▼▼▼
    [Header("Homing Lifetime")]
    [Tooltip("호밍을 일정 시간 뒤 자동으로 끌지 여부")]
    public bool useHomingTimeout = false;
    [Tooltip("시작(활성화) 후 이 시간(초)이 지나면 호밍 중지")]
    [Min(0f)] public float homingDuration = 3f;
    [Tooltip("호밍이 꺼지면 현재 속도로 직진 유지 (끄면 감속/정지 성향)")]
    public bool keepVelocityAfterTimeout = true;
    // ▲▲▲ 추가 끝 ▲▲▲

    Transform _target;
    [SerializeField] Rigidbody2D _rb;
    float _refreshTimer;
    Vector2 _smoothedTarget;
    Vector2 _lastKnown;
    float _lostTimer;

    Vector3 _cachedPosFromRb;

    // 내부: 목표 속도 버퍼
    Vector2 _desiredVel;

    // ▼▼▼ 추가: 경과 시간/호밍 활성 상태 ▼▼▼
    float _elapsed;
    bool _homingActive;
    // ▲▲▲ 추가 끝 ▲▲▲

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;

        // 참고: Unity 버전에 따라 아래 프로퍼티 명칭이 다를 수 있습니다.
        // (Unity 2022.x 등: drag / angularDrag / velocity)
#if UNITY_6000_0_OR_NEWER
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0.05f;
#else
        _rb.drag = 0f;
        _rb.angularDrag = 0.05f;
#endif
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected override void OnActivated()
    {
        // 속도 리셋
#if UNITY_6000_0_OR_NEWER
        _rb.linearVelocity = Vector2.zero;
#else
        _rb.velocity = Vector2.zero;
#endif
        _refreshTimer = 0f;
        _smoothedTarget = transform.position;
        _lastKnown = _smoothedTarget;
        _lostTimer = 0f;

        // ▼▼▼ 추가: 호밍 타이머 초기화 ▼▼▼
        _elapsed = 0f;
        _homingActive = !useHomingTimeout || homingDuration > 0f;
        // ▲▲▲

        _target = explicitTarget;
        if (_target == null && autoFindPlayer)
        {
            var p = GameObject.FindGameObjectWithTag(string.IsNullOrEmpty(targetTag) ? "Player" : targetTag);
            if (p) _target = p.transform;
        }
    }

    public override float Tick(ref Vector3 pos, float dt)
    {
        _cachedPosFromRb = _rb.position;
        pos = _cachedPosFromRb;

        // ▼▼▼ 추가: 호밍 시간 경과 체크 ▼▼▼
        if (useHomingTimeout)
        {
            _elapsed += dt;
            if (_homingActive && _elapsed >= homingDuration)
            {
                _homingActive = false;   // 호밍 종료
            }
        }
        // ▲▲▲

        // 호밍이 꺼졌다면: 더 이상 타깃 갱신/재탐색/조향을 하지 않음
        if (!_homingActive)
        {
            // 직진 유지(현재 속도 유지) 또는 감속 성향
#if UNITY_6000_0_OR_NEWER
            Vector2 cur = _rb.linearVelocity;
#else
            Vector2 cur = _rb.velocity;
#endif
            _desiredVel = keepVelocityAfterTimeout ? cur : Vector2.zero;
            return 0f;
        }

        // --- 아래는 "호밍 활성 상태"에서만 실행 ---
        if (_target == null && reacquireOnNull) TryRefresh(true);
        else TryRefresh(false);

        if (_target != null && maxChaseDistance > 0f)
        {
            float d = Vector2.Distance(_rb.position, _target.position);
            if (d > maxChaseDistance) _target = null;
        }

        ComputeDesiredVelocity(dt);

        return 0f;
    }

    void TryRefresh(bool force)
    {
        if (!refreshByTag) return;

        _refreshTimer -= Time.deltaTime;
        if (!force && _refreshTimer > 0f) return;

        _refreshTimer = refreshInterval;

        if (_target == null || force)
        {
            var go = GameObject.FindGameObjectWithTag(string.IsNullOrEmpty(targetTag) ? "Player" : targetTag);
            _target = go ? go.transform : null;
        }
    }

    Vector2 GetTargetPos(float dt)
    {
        if (_target == null) return _smoothedTarget;

        Vector2 p = (Vector2)_target.position + aimPointOffset;
        if (targetSmooth > 0f)
        {
            float k = 1f - Mathf.Exp(-targetSmooth * dt);
            _smoothedTarget = Vector2.Lerp(_smoothedTarget, p, k);
        }
        else _smoothedTarget = p;

        _lastKnown = _smoothedTarget;
        return _smoothedTarget;
    }

    void ComputeDesiredVelocity(float dt)
    {
        Vector2 myPos = _rb.position;

        if (_target != null)
        {
            _lostTimer = 0f;
            Vector2 target = GetTargetPos(dt);
            Vector2 to = target - myPos;
            float dist = to.magnitude;

            if (dist <= arriveStopDistance)
            {
                _desiredVel = Vector2.zero; // 도착
            }
            else
            {
                Vector2 dir = to / Mathf.Max(dist, 0.0001f);
                float desiredSpeed = maxSpeed;
                if (dist < slowRadius)
                    desiredSpeed = Mathf.Lerp(0f, maxSpeed, (dist - arriveStopDistance) / Mathf.Max(0.0001f, (slowRadius - arriveStopDistance)));
                desiredSpeed = Mathf.Clamp(desiredSpeed, 0f, maxSpeed);
                _desiredVel = dir * desiredSpeed;
            }
        }
        else
        {
            _lostTimer += dt;
            Vector2 to = _lastKnown - myPos;
            float dist = to.magnitude;

            if (_lostTimer <= lostPursuitTimeout && dist > arriveStopDistance)
            {
                Vector2 dir = to / Mathf.Max(dist, 0.0001f);
                float desiredSpeed = maxSpeed;
                if (dist < slowRadius)
                    desiredSpeed = Mathf.Lerp(0f, maxSpeed, (dist - arriveStopDistance) / Mathf.Max(0.0001f, (slowRadius - arriveStopDistance)));
                desiredSpeed = Mathf.Clamp(desiredSpeed, 0f, maxSpeed);
                _desiredVel = dir * desiredSpeed;
            }
            else
            {
                _desiredVel = Vector2.zero;
            }
        }
    }

    void FixedUpdate()
    {
        // 원하는 속도 → 현재 속도 차이를 스티어링 가속도로 변환
#if UNITY_6000_0_OR_NEWER
        Vector2 curVel = _rb.linearVelocity;
#else
        Vector2 curVel = _rb.velocity;
#endif
        Vector2 steering = _desiredVel - curVel;

        float maxDeltaV = maxAccel * Time.fixedDeltaTime;
        float mag = steering.magnitude;
        if (mag > maxDeltaV)
            steering = steering * (maxDeltaV / Mathf.Max(mag, 0.0001f));

        _rb.AddForce(steering * _rb.mass / Time.fixedDeltaTime, forceMode);

        // 속도 상한
#if UNITY_6000_0_OR_NEWER
        float spd = _rb.linearVelocity.magnitude;
        if (spd > maxSpeed)
            _rb.linearVelocity = _rb.linearVelocity * (maxSpeed / spd);
#else
        float spd = _rb.velocity.magnitude;
        if (spd > maxSpeed)
            _rb.velocity = _rb.velocity * (maxSpeed / spd);
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _target.position);
            Gizmos.DrawWireSphere(_target.position, 0.15f);
        }
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, slowRadius);
    }
#endif
}
