using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public enum DirectionMode { UseTransformUp, UseTransformRight, WorldUp, WorldRight, AimAtMouse }

    [Header("Refs")]
    BulletPoolHub pool;
    BulletPoolKey poolKey = BulletPoolKey.Player; // 인스펙터에서 Player/Enemy 선택
    public Transform muzzle;          // 총구(없으면 본인 transform 사용)

    [Header("Fire")]
    public float fireRate = 10f;              // 초당 발사 수
    public float bulletSpeed = 12f;
    public float bulletLifetime = 5f;
    public float bulletSize = 1f;
    public float bulletDamage;
    public float focusDamageMultiply;

    [Header("Direction")]
    public DirectionMode directionMode = DirectionMode.UseTransformUp;

    [Header("Optional")]
    public Vector2 spawnOffset = Vector2.zero;// 총구 기준 추가 오프셋

    [Header("Input (Hold-to-Fire)")]
    [Tooltip("해당 키를 누르고 있는 동안에만 발사합니다.")]
    public Key holdKey = Key.Space;           // 인스펙터에서 키 지정

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

        // 키가 눌려있지 않으면 발사하지 않음
        if (!IsHoldKeyPressed()) return;

        // 외부에서 발사 토글을 꺼두면 발사하지 않음
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
        // Keyboard.current가 null일 수 있는 에디터/플랫폼 대비
        if (kb == null) return false;

        // 새 입력 시스템: 인덱서로 KeyControl 접근 가능
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

        float deg = Mathf.Atan2(dir.y, dir.x); // Spawn 쪽 규약에 맞게 유지 (라디안/디그리는 기존 로직 준수)
        var b = pool.Spawn(poolKey, origin, dir.normalized * bulletSpeed, bulletLifetime, bulletDamage, deg);
    }

    // 외부에서 토글하고 싶다면:
    public void SetFiring(bool on) => _wantsFire = on;
}
