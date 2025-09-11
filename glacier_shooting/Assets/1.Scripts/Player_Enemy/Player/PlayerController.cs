// File: PlayerControllerSnappy.cs
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Top Speed")]
    public float speed = 6f;            // 일반 최대 속도
    public float focusSpeed = 3f;       // LeftShift(집중) 속도

    [Header("Feel (쫀득 파라미터)")]
    public float accel = 30f;           // 가속
    public float decel = 35f;           // 감속
    public float inputSmooth = 12f;     // 입력 스무딩 정도
    public float stopEpsilon = 0.01f;   // 정지 스냅 임계치

    [Header("Etc")]
    public bool normalizeDiagonal = true;
    public Vector2 worldClamp = new Vector2(999, 999); // 경계 제한 (999는 사실상 무제한)

    // 내부 상태
    Vector2 _vel;
    Vector2 _inSm;

    void Update()
    {
        // ── 1) 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inRaw = new Vector2(h, v);

        if (normalizeDiagonal && inRaw.sqrMagnitude > 1f)
            inRaw.Normalize();

        // 입력 스무딩
        float t = 1f - Mathf.Exp(-inputSmooth * Time.deltaTime);
        _inSm = Vector2.Lerp(_inSm, inRaw, t);

        // 목표 속도
        float maxSpd = Input.GetKey(KeyCode.LeftShift) ? focusSpeed : speed;
        Vector2 desiredVel = _inSm * maxSpd;

        // ── 2) 속도 보간 (가속/감속)
        float a = desiredVel.sqrMagnitude > 0.001f ? accel : decel;
        _vel = Vector2.MoveTowards(_vel, desiredVel, a * Time.deltaTime);

        // 정지 스냅
        if (_vel.sqrMagnitude < stopEpsilon * stopEpsilon && desiredVel == Vector2.zero)
            _vel = Vector2.zero;

        // ── 3) 실제 이동
        transform.position += (Vector3)(_vel * Time.deltaTime);

        // (선택) 경계 제한
        if (worldClamp.x < 900f)
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
            p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
            transform.position = p;
        }
    }
}
