using UnityEngine;

[DisallowMultipleComponent]
public class BossHomingShooter : MonoBehaviour
{
    [Header("Hub Reference")]
    public BulletPoolHub hub;
    public BulletPoolKey poolKey = BulletPoolKey.Enemy_Homing;

    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Bullet Settings")]
    public float speed = 5f;
    public float lifetime = 5f;
    public float damage = 1f;

    [Header("Timing")]
    [Min(0.1f)] public float coolDown = 2.0f;
    float _nextFire;

    void Awake()
    {
        if (!hub) hub = FindAnyObjectByType<BulletPoolHub>();
    }

    public void FireOnce()
    {
        if (!hub) return;
        if (Time.time < _nextFire) return;

        Vector3 pos = firePoint ? firePoint.position : transform.position;
        Quaternion rot = firePoint ? firePoint.rotation : transform.rotation;

        // 발사 방향 계산
        Vector2 initVel = (Vector2)(rot * Vector3.down) * speed;

        Bullet b = hub.Spawn(poolKey, pos, initVel, lifetime, damage, rot.eulerAngles.z);
        if (b)
        {
            b.SetHomingEnabled(true);
        }

        _nextFire = Time.time + coolDown;
    }
}
