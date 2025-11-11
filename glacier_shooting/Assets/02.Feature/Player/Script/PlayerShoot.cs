using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance { get; private set; }

    public enum DirectionMode { UseTransformUp, UseTransformRight, WorldUp, WorldRight, AimAtMouse }

    [Header("Refs")]
    BulletPoolHub pool;
    BulletPoolKey poolKey = BulletPoolKey.Player; // 인스펙터에서 Player/Enemy 선택
    public Transform muzzle;          // 총구(없으면 본인 transform 사용)

    [Header("Fire")]
    public float fireRate = 10f;              // 초당 발사 수(= 초당 연사 '볼리' 수)
    public float bulletSpeed = 12f;
    public float bulletLifetime = 5f;
    public float bulletSize = 1f;
    public float bulletDamage;
    public float focusDamageMultiply;
    public int nthAttack;

    [Header("Direction")]
    public DirectionMode directionMode = DirectionMode.UseTransformUp;

    [Header("Spread (Multi-shot)")]
    [Tooltip("한 번에 몇 방향으로 쏠지(1=직선만)")]
    public int shotCount = 1;
    [Tooltip("여러 발을 쏠 때 전체로 퍼지는 각도(도 단위)")]
    public float spreadAngle = 30f;

    [Header("Optional")]
    public Vector2 spawnOffset = Vector2.zero;// 총구 기준 추가 오프셋

    [Header("Input (Hold-to-Fire)")]
    [Tooltip("해당 키를 누르고 있는 동안에만 발사합니다.")]
    [SerializeField] private InputActionReference shootAction;

    float _accum;
    bool _wantsFire;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        pool = GameObject.FindFirstObjectByType<BulletPoolHub>();
        _wantsFire = true;
        nthAttack = 0;
    }

    void Update()
    {
        if (GameManager.Instance) if (GameManager.Instance.Paused) return;
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
            FireVolley(); // 변경: N발 산탄 발사
        }
    }
    bool IsHoldKeyPressed()
    {
        // 누르고 있는 동안 true (토글 X, 홀드 감지)
        return shootAction != null
            && shootAction.action != null
            && shootAction.action.IsPressed();
    }

    // === 새로 추가: 방향 모드에 따른 기본 조준 벡터 계산 ===
    Vector2 GetBaseDirection()
    {
        switch (directionMode)
        {
            case DirectionMode.UseTransformUp:
                return (Vector2)transform.up;
            case DirectionMode.UseTransformRight:
                return (Vector2)transform.right;
            case DirectionMode.WorldUp:
                return Vector2.up;
            case DirectionMode.WorldRight:
                return Vector2.right;
            case DirectionMode.AimAtMouse:
                {
                    // 마우스 위치에서 월드 포인트로
                    var mouse = Mouse.current;
                    if (mouse != null && Camera.main != null)
                    {
                        Vector2 mousePos = mouse.position.ReadValue();
                        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z));
                        Vector2 from = muzzle ? (Vector2)muzzle.position : (Vector2)transform.position;
                        Vector2 d = (world - (Vector3)from);
                        if (d.sqrMagnitude > 0.0001f) return d.normalized;
                    }
                    // 실패 시 transform.up fallback
                    return (Vector2)transform.up;
                }
            default:
                return (Vector2)transform.up;
        }
    }

    // === 새로 추가: 한 번에 여러 발을 각도로 분배해 발사 ===
    void FireVolley()
    {
        int n = Mathf.Max(1, shotCount);

        nthAttack++;

        // 베이스 위치/방향
        Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)transform.position;
        origin += spawnOffset;

        Vector2 baseDir = GetBaseDirection();
        if (baseDir.sqrMagnitude < 0.0001f) baseDir = Vector2.up;

        // 베이스 각도(라디안) — Mathf.Atan2는 라디안 반환
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x);

        // 총 퍼짐 각(라디안)
        float totalRad = Mathf.Deg2Rad * Mathf.Max(0f, spreadAngle);

        if (n == 1 || totalRad <= 0.0001f)
        {
            // 직선 한 발
            FireOne(origin, baseDir);
            return;
        }

        // n발을 부채꼴로 균등 배치: [-total/2, +total/2] 구간에 n개
        float start = -totalRad * 0.5f;
        float step = (n > 1) ? (totalRad / (n - 1)) : 0f;

        for (int i = 0; i < n; i++)
        {
            float angle = baseAngle + start + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            FireOne(origin, dir);
        }
    }

    // === 변경: 발사 1발 (기존 FireOne의 위치/방향 계산 분리) ===
    void FireOne(Vector2 origin, Vector2 dir)
    {
        // 1) 발사 직전 요청 객체 만들고, 기본 배수 1로 초기화
        var req = new ShotRequest();

        // 2) 외부(유물 이펙트)들에게 수정 기회 제공
        GameEvents.RaiseBeforeBasicAttackFired(req);

        // 3) 요청 반영해서 최종 값 계산
        float finalDamage = bulletDamage * req.damageMul;
        float finalSpeed = bulletSpeed * req.speedMul;
        float finalLifetime = bulletLifetime * req.lifetimeMul;
        float finalSizeMul = bulletSize * req.sizeMul; // 1이면 변화 없음

        // 4) 탄 스폰 (기존 풀 규약 유지)
        float angRad = Mathf.Atan2(dir.y, dir.x); // 라디안
        var b = pool.Spawn(poolKey, origin, dir.normalized * finalSpeed, finalLifetime, finalDamage, angRad);

        // 5) 색/크기 등 비주얼 적용
        if (b != null)
        {
            b.GetComponent<Bullet>().UpdateHitRadius(finalSizeMul);

            var vis = b.GetComponent<BulletVisual>();
            if (vis != null)
            {
                // 항상 기본 상태로 초기화
                vis.ResetVisuals();

                // tint가 있을 때만 덮어쓰기
                if (req.tint.HasValue)
                    vis.ApplyTint(req.tint.Value);

                // 사이즈 배율 적용 (누적 X)
                if (Mathf.Abs(finalSizeMul - 1f) > 0.0001f)
                    vis.ApplySizeMul(finalSizeMul);
            }
            else
            {
                // BulletVisual이 없다면 최소한의 방어 코드
                var sr = b.GetComponentInChildren<SpriteRenderer>();
                if (sr && req.tint.HasValue)
                    sr.color = req.tint.Value;

                if (Mathf.Abs(finalSizeMul - 1f) > 0.0001f)
                    b.transform.localScale *= finalSizeMul; // <- 가능하면 누적되지 않게 수정 권장
            }
        }

        // 6) 사후 이벤트(카운트/로그 등)
        GameEvents.RaiseBasicAttackFired();
    }

    // 외부에서 토글하고 싶다면:
    public void SetFiring(bool on) => _wantsFire = on;
}
