using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public enum DirectionMode { UseTransformUp, UseTransformRight, WorldUp, WorldRight, AimAtMouse }

    [Header("Refs")]
    BulletPoolHub pool;
    BulletPoolKey poolKey = BulletPoolKey.Player; // �ν����Ϳ��� Player/Enemy ����
    public Transform muzzle;          // �ѱ�(������ ���� transform ���)

    [Header("Fire")]
    public float fireRate = 10f;              // �ʴ� �߻� ��
    public float bulletSpeed = 12f;
    public float bulletLifetime = 5f;
    public float bulletSize = 1f;
    public float bulletDamage;
    public float focusDamageMultiply;

    [Header("Direction")]
    public DirectionMode directionMode = DirectionMode.UseTransformUp;

    [Header("Optional")]
    public Vector2 spawnOffset = Vector2.zero;// �ѱ� ���� �߰� ������

    [Header("Input (Hold-to-Fire)")]
    [Tooltip("�ش� Ű�� ������ �ִ� ���ȿ��� �߻��մϴ�.")]
    public Key holdKey = Key.Space;           // �ν����Ϳ��� Ű ����

    float _accum;
    bool _wantsFire;

    void OnEnable()
    {
        pool = GameObject.FindFirstObjectByType<BulletPoolHub>();
        _wantsFire = true;
    }

    void Update()
    {
        if (GameManager.Instance.Paused) return;
        if (pool == null) return;

        // Ű�� �������� ������ �߻����� ����
        if (!IsHoldKeyPressed()) return;

        // �ܺο��� �߻� ����� ���θ� �߻����� ����
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

    bool IsHoldKeyPressed()
    {
        var kb = Keyboard.current;
        // Keyboard.current�� null�� �� �ִ� ������/�÷��� ���
        if (kb == null) return false;

        // �� �Է� �ý���: �ε����� KeyControl ���� ����
        var keyCtrl = kb[holdKey];
        return keyCtrl != null && keyCtrl.isPressed;
    }

    void FireOne()
    {
        //if (PlayerStatus.Instance.PlayerMana <= 0) return;
        //PlayerStatus.Instance.PlayerMana--;
        Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)transform.position;
        origin += spawnOffset;

        Vector2 dir = transform.up;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        float deg = Mathf.Atan2(dir.y, dir.x); // Spawn �� �Ծ࿡ �°� ���� (����/��׸��� ���� ���� �ؼ�)
        var b = pool.Spawn(poolKey, origin, dir.normalized * bulletSpeed, bulletLifetime, bulletDamage, deg);
    }

    // �ܺο��� ����ϰ� �ʹٸ�:
    public void SetFiring(bool on) => _wantsFire = on;
}
