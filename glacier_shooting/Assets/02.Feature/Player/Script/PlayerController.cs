// File: PlayerControllerSnappy.cs
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Top Speed")]
    public float speed = 6f;
    public float focusSpeed = 3f;   // Shift로 느리게

    [Header("Feel")]
    public float accel = 30f;
    public float decel = 35f;
    public float inputSmooth = 12f;
    public float stopEpsilon = 0.01f;
    public bool normalizeDiagonal = true;

    [Header("Dash (I-frame)")]
    public float dashSpeed = 16f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public float invincibleExtra = 0.05f; // 대시 끝나고 약간 더 무적

    [Header("Bounds (Manual Rect)")]
    [Tooltip("경계 중심(비우면 (0,0) 기준)")]
    public Transform areaCenter;
    [Tooltip("경계 크기 (가로, 세로)")]
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Refs")]
    public VerticalScrollerSimple vsSample;

    // 내부
    [SerializeField] Vector2 _vel;
    [SerializeField] Vector2 _inSm;
    [SerializeField] Vector2 _lastMoveDir = Vector2.up;  // 입력 없을 때 대시 방향
    [SerializeField] bool _isDashing = false;
    [SerializeField] bool _dashOnCooldown = false;

    [Header("PlayerInput (New Input System)")]
    [SerializeField] private InputActionReference moveAction;   // Vector2
    [SerializeField] private InputActionReference dashAction;   // Button
    [SerializeField] private InputActionReference focusAction;  // Button (느린이동)

    PlayerStatus _status;

    void Awake()
    {
        _status = PlayerStatus.Instance;
        if (!_status) _status = GetComponent<PlayerStatus>();
    }

    void OnEnable()
    {
        // 액션 활성화
        moveAction?.action?.Enable();
        dashAction?.action?.Enable();
        focusAction?.action?.Enable();
    }

    void OnDisable()
    {
        // 액션 비활성화
        moveAction?.action?.Disable();
        dashAction?.action?.Disable();
        focusAction?.action?.Disable();
    }

    void Update()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.Paused) return;

        // ── 대시 입력 체크 (New Input System)
        bool dashPressed = dashAction != null && dashAction.action != null
            && dashAction.action.WasPressedThisFrame();
        if (dashPressed)
            TryDash();

        if (_isDashing)
        {
            transform.position = ClampToBounds(transform.position);
            return;
        }

        // ── 일반 이동 (New Input System)
        Vector2 inRaw = Vector2.zero;
        if (moveAction != null && moveAction.action != null)
            inRaw = moveAction.action.ReadValue<Vector2>(); // ← GetAxisRaw 대체

        if (normalizeDiagonal && inRaw.sqrMagnitude > 1f)
            inRaw.Normalize();

        // 입력 스무딩
        float t = 1f - Mathf.Exp(-inputSmooth * Time.deltaTime);
        _inSm = Vector2.Lerp(_inSm, inRaw, t);

        // 방향 기록(대시 방향에 사용)
        if (_inSm.sqrMagnitude > 0.0001f) _lastMoveDir = _inSm.normalized;

        // 느린 이동(포커스) 여부
        bool focusHeld = focusAction != null && focusAction.action != null
            && focusAction.action.IsPressed();
        float maxSpd = focusHeld ? focusSpeed : speed;

        // 목표 속도
        Vector2 desiredVel = _inSm * maxSpd;

        // 가속/감속
        float a = desiredVel.sqrMagnitude > 0.001f ? accel : decel;
        _vel = Vector2.MoveTowards(_vel, desiredVel, a * Time.deltaTime);

        // 정지 스냅
        if (_vel.sqrMagnitude < stopEpsilon * stopEpsilon && desiredVel == Vector2.zero)
            _vel = Vector2.zero;

        // 이동 + 클램프
        Vector3 next = transform.position + (Vector3)(_vel * Time.deltaTime);
        transform.position = ClampToBounds(next);
    }

    void TryDash()
    {
        if (_isDashing || _dashOnCooldown) return;

        // 방향이 0이면 마지막 이동 방향(기본 위)
        Vector2 dir = (_inSm.sqrMagnitude > 0.0001f) ? _inSm.normalized : _lastMoveDir;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        StartCoroutine(CoDash(dir));
    }

    IEnumerator CoDash(Vector2 dir)
    {
        _isDashing = true;
        _dashOnCooldown = true;

        GameEvents.RaiseDashed();
        if (_status) _status.invincible = true;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            Vector3 next = transform.position + (Vector3)(dir * dashSpeed * Time.deltaTime);
            transform.position = ClampToBounds(next);
            yield return null;
        }

        if (invincibleExtra > 0f)
            yield return new WaitForSeconds(invincibleExtra);

        if (_status) _status.invincible = false;
        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        _dashOnCooldown = false;
    }

    // ───────────────────────────────────────────────────────────────
    Vector3 ClampToBounds(Vector3 pos)
    {
        Vector2 half = rectSize * 0.5f;
        Vector3 c = areaCenter ? areaCenter.position : Vector3.zero;
        pos.x = Mathf.Clamp(pos.x, c.x - half.x, c.x + half.x);
        pos.y = Mathf.Clamp(pos.y, c.y - half.y, c.y + half.y);
        return pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Vector3 c = areaCenter ? areaCenter.position : Vector3.zero;
        Gizmos.DrawCube(c, new Vector3(rectSize.x, rectSize.y, 0.01f));
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
        Gizmos.DrawWireCube(c, new Vector3(rectSize.x, rectSize.y, 0.01f));
    }
#endif
}
