// File: PlayerControllerSnappy.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Top Speed")]
    public float speed = 6f;
    public float focusSpeed = 3f;   // LeftShift로 느리게

    [Header("Feel")]
    public float accel = 30f;
    public float decel = 35f;
    public float inputSmooth = 12f;
    public float stopEpsilon = 0.01f;
    public bool normalizeDiagonal = true;

    [Header("Dash (I-frame)")]
    public KeyCode dashKey = KeyCode.Space;
    public float dashSpeed = 16f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public float invincibleExtra = 0.05f; // 대시 끝나고 약간 더 무적

    [Header("Clamp (optional)")]
    public Vector2 worldClamp = new Vector2(999, 999);

    // 내부
    Vector2 _vel;
    Vector2 _inSm;
    Vector2 _lastMoveDir = Vector2.up;  // 입력 없을 때 대시 방향
    bool _isDashing = false;
    bool _dashOnCooldown = false;

    PlayerStatus _status;

    void Awake()
    {
        _status = PlayerStatus.Instance; // 있으면 자동
        if (!_status) _status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        // ── 대시 입력 체크 (이동보다 우선)
        if (Input.GetKeyDown(dashKey))
            TryDash();

        if (_isDashing)
        {
            // 대시 중엔 이동 갱신 안 함(코루틴에서 속도 설정)
            if (worldClamp.x < 900f)
            {
                Vector3 p = transform.position;
                p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
                p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
                transform.position = p;
            }
            return;
        }

        // ── 일반 이동
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inRaw = new Vector2(h, v);
        if (normalizeDiagonal && inRaw.sqrMagnitude > 1f) inRaw.Normalize();

        // 입력 스무딩
        float t = 1f - Mathf.Exp(-inputSmooth * Time.deltaTime);
        _inSm = Vector2.Lerp(_inSm, inRaw, t);

        // 방향 기록(대시 방향에 사용)
        if (_inSm.sqrMagnitude > 0.0001f) _lastMoveDir = _inSm.normalized;

        // 목표 속도
        float maxSpd = Input.GetKey(KeyCode.LeftShift) ? focusSpeed : speed;
        Vector2 desiredVel = _inSm * maxSpd;

        // 가속/감속
        float a = desiredVel.sqrMagnitude > 0.001f ? accel : decel;
        _vel = Vector2.MoveTowards(_vel, desiredVel, a * Time.deltaTime);

        // 정지 스냅
        if (_vel.sqrMagnitude < stopEpsilon * stopEpsilon && desiredVel == Vector2.zero)
            _vel = Vector2.zero;

        // 이동
        transform.position += (Vector3)(_vel * Time.deltaTime);

        // 경계
        if (worldClamp.x < 900f)
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
            p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
            transform.position = p;
        }
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

        // 무적 ON
        if (_status) _status.invincible = true;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            transform.position += (Vector3)(dir * dashSpeed * Time.deltaTime);

            // (선택) 대시 도중에도 경계 유지
            if (worldClamp.x < 900f)
            {
                Vector3 p = transform.position;
                p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
                p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
                transform.position = p;
            }

            yield return null;
        }

        // 약간 더 무적 유지(히트 판정 여유)
        if (invincibleExtra > 0f)
            yield return new WaitForSeconds(invincibleExtra);

        // 무적 OFF
        if (_status) _status.invincible = false;

        _isDashing = false;

        // 쿨다운
        yield return new WaitForSeconds(dashCooldown);
        _dashOnCooldown = false;
    }
}
