// File: PlayerControllerSnappy.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Top Speed")]
    public float speed = 6f;
    public float focusSpeed = 3f;   // LeftShift�� ������

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
    public float invincibleExtra = 0.05f; // ��� ������ �ణ �� ����

    [Header("Clamp (optional)")]
    public Vector2 worldClamp = new Vector2(999, 999);

    // ����
    Vector2 _vel;
    Vector2 _inSm;
    Vector2 _lastMoveDir = Vector2.up;  // �Է� ���� �� ��� ����
    bool _isDashing = false;
    bool _dashOnCooldown = false;

    PlayerStatus _status;

    void Awake()
    {
        _status = PlayerStatus.Instance; // ������ �ڵ�
        if (!_status) _status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        // ���� ��� �Է� üũ (�̵����� �켱)
        if (Input.GetKeyDown(dashKey))
            TryDash();

        if (_isDashing)
        {
            // ��� �߿� �̵� ���� �� ��(�ڷ�ƾ���� �ӵ� ����)
            if (worldClamp.x < 900f)
            {
                Vector3 p = transform.position;
                p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
                p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
                transform.position = p;
            }
            return;
        }

        // ���� �Ϲ� �̵�
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inRaw = new Vector2(h, v);
        if (normalizeDiagonal && inRaw.sqrMagnitude > 1f) inRaw.Normalize();

        // �Է� ������
        float t = 1f - Mathf.Exp(-inputSmooth * Time.deltaTime);
        _inSm = Vector2.Lerp(_inSm, inRaw, t);

        // ���� ���(��� ���⿡ ���)
        if (_inSm.sqrMagnitude > 0.0001f) _lastMoveDir = _inSm.normalized;

        // ��ǥ �ӵ�
        float maxSpd = Input.GetKey(KeyCode.LeftShift) ? focusSpeed : speed;
        Vector2 desiredVel = _inSm * maxSpd;

        // ����/����
        float a = desiredVel.sqrMagnitude > 0.001f ? accel : decel;
        _vel = Vector2.MoveTowards(_vel, desiredVel, a * Time.deltaTime);

        // ���� ����
        if (_vel.sqrMagnitude < stopEpsilon * stopEpsilon && desiredVel == Vector2.zero)
            _vel = Vector2.zero;

        // �̵�
        transform.position += (Vector3)(_vel * Time.deltaTime);

        // ���
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

        // ������ 0�̸� ������ �̵� ����(�⺻ ��)
        Vector2 dir = (_inSm.sqrMagnitude > 0.0001f) ? _inSm.normalized : _lastMoveDir;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        StartCoroutine(CoDash(dir));
    }

    IEnumerator CoDash(Vector2 dir)
    {
        _isDashing = true;
        _dashOnCooldown = true;

        // ���� ON
        if (_status) _status.invincible = true;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            transform.position += (Vector3)(dir * dashSpeed * Time.deltaTime);

            // (����) ��� ���߿��� ��� ����
            if (worldClamp.x < 900f)
            {
                Vector3 p = transform.position;
                p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
                p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
                transform.position = p;
            }

            yield return null;
        }

        // �ణ �� ���� ����(��Ʈ ���� ����)
        if (invincibleExtra > 0f)
            yield return new WaitForSeconds(invincibleExtra);

        // ���� OFF
        if (_status) _status.invincible = false;

        _isDashing = false;

        // ��ٿ�
        yield return new WaitForSeconds(dashCooldown);
        _dashOnCooldown = false;
    }
}
