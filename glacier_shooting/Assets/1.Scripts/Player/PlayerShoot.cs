using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public enum DirectionMode { UseTransformUp, UseTransformRight, WorldUp, WorldRight, AimAtMouse }

    [Header("Refs")]
    public BulletPool pool;           // ������ ���� BulletPool ����
    public Transform muzzle;          // �ѱ�(������ ���� transform ���)

    [Header("Fire")]
    public bool autoFireOnStart = true;       // ���ۺ��� �ڵ����
    public bool holdToFire = false;           // �Է� Ȧ��θ� �߻�����
    public InputActionReference fireAction;   // New Input System�� Fire �׼�(��ư)
    public float fireRate = 10f;              // �ʴ� �߻� ��
    public float bulletSpeed = 12f;
    public float bulletLifetime = 5f;

    [Header("Direction")]
    public DirectionMode directionMode = DirectionMode.UseTransformUp;

    [Header("Optional")]
    public int overrideBulletLayer = -1;      // -1�̸� ����, �ƴϸ� ���� ���� ���̾� ����
    public string overrideBulletTag = "";     // �� ���ڿ��̸� ����, �ƴϸ� ���� ���� �±� ����
    public Vector2 spawnOffset = Vector2.zero;// �ѱ� ���� �߰� ������

    float _accum;
    bool _wantsFire;

    void OnEnable()
    {
        _wantsFire = true;

        if (holdToFire && fireAction != null)
        {
            var a = fireAction.action;
            a.started += OnFireStarted;   // ��ư ���� ����
            a.canceled += OnFireCanceled;  // ��ư ��
            if (!a.enabled) a.Enable();
        }
    }

    void OnDisable()
    {
        if (holdToFire && fireAction != null)
        {
            var a = fireAction.action;
            a.started -= OnFireStarted;
            a.canceled -= OnFireCanceled;
        }
    }

    void Update()
    {
        if (pool == null) return;
        if (!_wantsFire) return;

        float dt = Time.deltaTime;
        _accum += dt;

        float interval = 1f / Mathf.Max(0.0001f, fireRate);
        while (_accum >= interval)
        {
            _accum -= interval;
            FireOne();
        }
    }

    void FireOne()
    {
        Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)transform.position;
        origin += spawnOffset;

        Vector2 dir = transform.up;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        float deg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        var b = pool.Spawn(origin, dir.normalized * bulletSpeed, bulletLifetime, zRotationDeg: deg);
        if (b != null)
        {
            if (overrideBulletLayer >= 0) b.gameObject.layer = overrideBulletLayer;
            if (!string.IsNullOrEmpty(overrideBulletTag)) b.gameObject.tag = overrideBulletTag;
        }
    }

    void OnFireStarted(InputAction.CallbackContext _)
    {
        //if (holdToFire) _wantsFire = true;
    }

    void OnFireCanceled(InputAction.CallbackContext _)
    {
        //if (holdToFire) _wantsFire = false;
    }

    // �ܺο��� ����ϰ� �ʹٸ�:
    public void SetFiring(bool on) => _wantsFire = on;
}
