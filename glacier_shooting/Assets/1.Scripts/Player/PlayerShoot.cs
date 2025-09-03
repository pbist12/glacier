using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public enum DirectionMode { UseTransformUp, UseTransformRight, WorldUp, WorldRight, AimAtMouse }

    [Header("Refs")]
    public BulletPool pool;           // 기존에 만든 BulletPool 참조
    public Transform muzzle;          // 총구(없으면 본인 transform 사용)

    [Header("Fire")]
    public bool autoFireOnStart = true;       // 시작부터 자동사격
    public bool holdToFire = false;           // 입력 홀드로만 발사할지
    public InputActionReference fireAction;   // New Input System의 Fire 액션(버튼)
    public float fireRate = 10f;              // 초당 발사 수
    public float bulletSpeed = 12f;
    public float bulletLifetime = 5f;

    [Header("Direction")]
    public DirectionMode directionMode = DirectionMode.UseTransformUp;

    [Header("Optional")]
    public int overrideBulletLayer = -1;      // -1이면 무시, 아니면 스폰 직후 레이어 지정
    public string overrideBulletTag = "";     // 빈 문자열이면 무시, 아니면 스폰 직후 태그 지정
    public Vector2 spawnOffset = Vector2.zero;// 총구 기준 추가 오프셋

    float _accum;
    bool _wantsFire;

    void OnEnable()
    {
        _wantsFire = true;

        if (holdToFire && fireAction != null)
        {
            var a = fireAction.action;
            a.started += OnFireStarted;   // 버튼 눌림 시작
            a.canceled += OnFireCanceled;  // 버튼 뗌
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

    // 외부에서 토글하고 싶다면:
    public void SetFiring(bool on) => _wantsFire = on;
}
