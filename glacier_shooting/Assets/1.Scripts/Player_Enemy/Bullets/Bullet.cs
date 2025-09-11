// File: Bullet.cs
using UnityEngine;

[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    [HideInInspector] public BulletPoolHub hub;
    [HideInInspector] public BulletPoolKey poolKey;

    [Header("Common")]
    public float damage;
    public Vector2 velocity;
    public float lifetime = 4f;
    public bool despawnOnBorderExit = true;

    [Header("Homing")]
    public bool homingEnabled = false;
    public float speed = 5f;
    public float rotateSpeed = 200f;
    public float homingDelay = 0.6f;
    public float homingDuration = 1.8f;
    public string targetTag = "Player";

    [Header("Collision Debug")]
    public float hitRadius = 0.12f;
    [Tooltip("씬 뷰에서 판정 반경을 Gizmo로 표시할지 여부")]
    public bool drawHitRadiusGizmo = true;

    [HideInInspector] public Vector2 prevPos;

    // 내부
    float _age;
    float _phaseTimer;
    Transform _tf;
    Transform _target;

    enum State { StraightByVelocity, PreHoming, Homing, StraightFacing }
    State _state = State.StraightByVelocity;

    void Awake() => _tf = transform;

    void OnEnable()
    {
        _age = 0f;
        _phaseTimer = 0f;

        // ★ 풀에서 다시 살아날 때 이전 위치가 남지 않도록
        prevPos = transform.position;

        if (homingEnabled)
        {
            if (_target == null)
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) _target = go.transform;
            }
            _state = State.PreHoming;
            if (speed <= 0f) speed = velocity.magnitude;
        }
        else
        {
            _state = State.StraightByVelocity;
        }
    }

    void Update()
    {
        _age += Time.deltaTime;
        if (_age >= lifetime) { Despawn(); return; }

        // ★ 이동 전에 항상 prevPos를 현재 위치로 저장
        prevPos = _tf.position;

        _phaseTimer += Time.deltaTime;

        if (!homingEnabled)
        {
            _tf.position += (Vector3)(velocity * Time.deltaTime);
            return;
        }

        switch (_state)
        {
            case State.PreHoming:
                MoveForwardFacing();
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

                Vector2 dir = (_target.position - _tf.position).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
                float newAngle = Mathf.MoveTowardsAngle(
                    _tf.eulerAngles.z, targetAngle, rotateSpeed * Time.deltaTime
                );
                _tf.rotation = Quaternion.Euler(0, 0, newAngle);

                MoveForwardFacing();

                if (homingDuration > 0f && _phaseTimer >= homingDuration)
                {
                    _state = State.StraightFacing;
                }
                break;

            case State.StraightFacing:
                MoveForwardFacing();
                break;

            case State.StraightByVelocity:
                _tf.position += (Vector3)(velocity * Time.deltaTime);
                break;
        }
    }

    public void Despawn()
    {
        if (!gameObject.activeSelf) return;
        hub.Despawn(this);
    }

    void MoveForwardFacing()
    {
        _tf.position += -_tf.up * speed * Time.deltaTime;
    }

    public void SetHomingEnabled(bool on, Transform targetOverride = null)
    {
        homingEnabled = on;
        if (targetOverride) _target = targetOverride;

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

    public void SetTarget(Transform t) => _target = t;

    // ===== 디버그 Gizmos =====
    void OnDrawGizmos()
    {
        if (!drawHitRadiusGizmo) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
