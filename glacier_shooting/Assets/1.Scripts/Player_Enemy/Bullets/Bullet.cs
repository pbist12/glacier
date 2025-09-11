// File: Bullet.cs
using UnityEngine;

[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    [HideInInspector] public BulletPoolHub hub;        // 허브로 복귀
    [HideInInspector] public BulletPoolKey poolKey;    // 내 소속 풀 키

    [Header("Common")]
    public float damage;
    public Vector2 velocity; // 일반탄: 초당 이동 벡터
    public float lifetime = 4f; // 수명(초)
    public bool despawnOnBorderExit = true; // Border 나가면 자동 회수

    [Header("Homing")]
    public bool homingEnabled = false;   // ✅ 유도 On/Off
    public float speed = 5f;             // 유도 이동 속도(유도 시 사용)
    public float rotateSpeed = 200f;     // 유도 회전 속도(도/초)
    public float homingDelay = 0.6f;     // 유도 시작 전 직진 시간
    public float homingDuration = 1.8f;  // 유도 유지 시간(<=0 이면 무제한)
    public string targetTag = "Player";  // 타깃 태그

    // 내부
    float _age;
    float _phaseTimer;
    Transform _tf;
    Transform _target;

    enum State { StraightByVelocity, PreHoming, Homing, StraightFacing }
    State _state = State.StraightByVelocity;

    void Awake()
    {
        _tf = transform;
    }

    void OnEnable()
    {
        _age = 0f;
        _phaseTimer = 0f;

        // 유도 모드라면 상태 초기화
        if (homingEnabled)
        {
            // 타깃 캐시(없으면 시도)
            if (_target == null)
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) _target = go.transform;
            }

            _state = State.PreHoming; // 초기엔 직진(회전 없이 -up)
            // 유도 속도가 0이면 기존 velocity에서 크기 추정
            if (speed <= 0f) speed = velocity.magnitude;
        }
        else
        {
            _state = State.StraightByVelocity; // 기존 방식
        }
    }

    void Update()
    {
        // 수명 체크
        _age += Time.deltaTime;
        if (_age >= lifetime)
        {
            Despawn();
            return;
        }

        _phaseTimer += Time.deltaTime;

        if (!homingEnabled)
        {
            // ── 일반탄: 기존 벡터 이동
            _tf.position += (Vector3)(velocity * Time.deltaTime);
            return;
        }

        // ── 유도탄 상태 머신
        switch (_state)
        {
            case State.PreHoming:
                MoveForwardFacing(); // 현재 바라보는 방향으로 직진(-up)
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
                    MoveForwardFacing();
                    break;
                }

                // 목표 각도(스프라이트가 ↓를 앞이라 가정: +90 보정)
                Vector2 dir = (_target.position - _tf.position).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                float newAngle = Mathf.MoveTowardsAngle(
                    _tf.eulerAngles.z, targetAngle, rotateSpeed * Time.deltaTime
                );
                _tf.rotation = Quaternion.Euler(0, 0, newAngle);

                MoveForwardFacing();

                if (homingDuration > 0f && _phaseTimer >= homingDuration)
                {
                    _state = State.StraightFacing; // 유도 종료 → 직진 유지
                }
                break;

            case State.StraightFacing:
                // 유도 종료 후: 현재 각도로 직진
                MoveForwardFacing();
                break;

            case State.StraightByVelocity:
                // 안전장치(이 상태는 homingEnabled=false일 때만)
                _tf.position += (Vector3)(velocity * Time.deltaTime);
                break;
        }
    }

    // 풀 복귀
    public void Despawn()
    {
        if (!gameObject.activeSelf) return;
        hub.Despawn(this);
    }

    // ----- 유틸 -----
    void MoveForwardFacing()
    {
        _tf.position += -_tf.up * speed * Time.deltaTime;
    }

    /// <summary>외부에서 유도 토글</summary>
    public void SetHomingEnabled(bool on, Transform targetOverride = null)
    {
        homingEnabled = on;
        if (targetOverride) _target = targetOverride;

        // 상태 즉시 갱신
        _phaseTimer = 0f;
        if (homingEnabled)
        {
            if (_target == null)
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) _target = go.transform;
            }
            if (speed <= 0f) speed = velocity.magnitude;
            _state = State.PreHoming;
        }
        else
        {
            _state = State.StraightByVelocity;
        }
    }

    /// <summary>목표 직접 지정</summary>
    public void SetTarget(Transform t)
    {
        _target = t;
    }
}
