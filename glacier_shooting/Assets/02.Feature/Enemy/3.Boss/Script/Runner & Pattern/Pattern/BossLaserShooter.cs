using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class BossLaserShooter : MonoBehaviour
{
    [Header("Setup")]
    public Transform muzzle;                  // 레이저 시작점(없으면 자기 Transform)
    public float maxDistance = 12f;
    public LayerMask hitMask;

    [Header("Visual")]
    public Color color = Color.red;
    [Min(0f)] public float width = 0.3f;
    public bool useLocalRightAsForward = true; // 2D 기준: muzzle.right 방향으로 쏨

    [Header("Damage (optional)")]
    public bool applyDamage = false;
    public float dps = 50f;

    private LineRenderer _lr;
    private bool _enabled;

    void Reset()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 2;
        _lr.useWorldSpace = true;
        _lr.startColor = _lr.endColor = color;
        _lr.startWidth = _lr.endWidth = width;
        muzzle = transform;
    }

    void Awake()
    {
        if (!_lr) _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;
    }

    void OnValidate()
    {
        if (!_lr) _lr = GetComponent<LineRenderer>();
        if (_lr)
        {
            _lr.startColor = _lr.endColor = color;
            _lr.startWidth = _lr.endWidth = width;
        }
    }

    public void Enable(bool on)
    {
        _enabled = on;
        if (_lr) _lr.enabled = on;
    }

    public void SetWidth(float w)
    {
        width = Mathf.Max(0f, w);
        if (_lr) { _lr.startWidth = _lr.endWidth = width; }
    }

    public void Sweep(float degDelta)
    {
        if (!muzzle) muzzle = transform;
        muzzle.Rotate(0f, 0f, degDelta, Space.Self);
    }

    public void SetDirection(Vector2 dir)
    {
        if (!muzzle) muzzle = transform;
        if (dir.sqrMagnitude > 0.0001f)
        {
            float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            muzzle.rotation = Quaternion.Euler(0, 0, z);
        }
    }

    void Update()
    {
        if (!_enabled || _lr == null) return;

        if (!muzzle) muzzle = transform;

        Vector3 origin = muzzle.position;
        Vector3 forward = useLocalRightAsForward ? muzzle.right : muzzle.up; // 2D: 보통 right

        float length = maxDistance;
        RaycastHit2D hit = Physics2D.Raycast(origin, forward, maxDistance, hitMask);
        if (hit.collider != null)
        {
            length = hit.distance;

            if (applyDamage)
            {
                var dmg = hit.collider.GetComponent<IDamageable>();
                if (dmg != null) dmg.TakeDamage(dps * Time.deltaTime);
            }
        }

        _lr.SetPosition(0, origin);
        _lr.SetPosition(1, origin + forward * length);
    }
}

/// <summary>선택: 간단한 데미지 인터페이스</summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
