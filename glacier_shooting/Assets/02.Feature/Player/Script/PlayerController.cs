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

    [Header("Bounds (Manual Rect)")]
    [Tooltip("��� �߽�(���� (0,0) ����)")]
    public Transform areaCenter;
    [Tooltip("��� ũ�� (����, ����)")]
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Refs")]
    public VerticalScrollerSimple vsSample;

    // ����
    [SerializeField] Vector2 _vel;
    [SerializeField] Vector2 _inSm;
    [SerializeField] Vector2 _lastMoveDir = Vector2.up;  // �Է� ���� �� ��� ����
    [SerializeField] bool _isDashing = false;
    [SerializeField] bool _dashOnCooldown = false;

    PlayerStatus _status;

    void Awake()
    {
        _status = PlayerStatus.Instance;
        if (!_status) _status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        if(GameManager.Instance) if (GameManager.Instance.Paused) return;
        // ���� ��� �Է� üũ (�̵����� �켱)
        if (Input.GetKeyDown(dashKey))
            TryDash();

        if (_isDashing)
        {
            transform.position = ClampToBounds(transform.position);
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

        // �̵� + Ŭ����
        Vector3 next = transform.position + (Vector3)(_vel * Time.deltaTime);
        transform.position = ClampToBounds(next);
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

    // ������������������������������������������������������������������������������������������������������������������������������
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
