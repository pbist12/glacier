using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public enum DirectionMode { UseTransformUp, UseTransformRight, WorldUp, WorldRight, AimAtMouse }

    [Header("Refs")]
    
    [SerializeField] BulletPoolHub pool;
    [SerializeField] BulletPoolKey poolKey = BulletPoolKey.Player; // �ν����Ϳ��� Player/Enemy ����
    public Transform muzzle;          // �ѱ�(������ ���� transform ���)

    [Header("Fire")]
    public float fireRate = 10f;              // �ʴ� �߻� ��
    public float bulletSpeed = 12f;
    public float bulletLifetime = 5f;

    [Header("Direction")]
    public DirectionMode directionMode = DirectionMode.UseTransformUp;

    [Header("Optional")]
    public Vector2 spawnOffset = Vector2.zero;// �ѱ� ���� �߰� ������

    float _accum;
    bool _wantsFire;

    void OnEnable()
    {
        _wantsFire = true;
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

        var b = pool.Spawn(poolKey, origin, dir.normalized * bulletSpeed, bulletLifetime, deg);
    }

    // �ܺο��� ����ϰ� �ʹٸ�:
    public void SetFiring(bool on) => _wantsFire = on;
}
