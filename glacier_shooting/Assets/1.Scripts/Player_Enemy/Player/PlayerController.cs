// File: PlayerControllerSnappy.cs
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Top Speed")]
    public float speed = 6f;            // �Ϲ� �ִ� �ӵ�
    public float focusSpeed = 3f;       // LeftShift(����) �ӵ�

    [Header("Feel (�˵� �Ķ����)")]
    public float accel = 30f;           // ����
    public float decel = 35f;           // ����
    public float inputSmooth = 12f;     // �Է� ������ ����
    public float stopEpsilon = 0.01f;   // ���� ���� �Ӱ�ġ

    [Header("Etc")]
    public bool normalizeDiagonal = true;
    public Vector2 worldClamp = new Vector2(999, 999); // ��� ���� (999�� ��ǻ� ������)

    // ���� ����
    Vector2 _vel;
    Vector2 _inSm;

    void Update()
    {
        // ���� 1) �Է�
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inRaw = new Vector2(h, v);

        if (normalizeDiagonal && inRaw.sqrMagnitude > 1f)
            inRaw.Normalize();

        // �Է� ������
        float t = 1f - Mathf.Exp(-inputSmooth * Time.deltaTime);
        _inSm = Vector2.Lerp(_inSm, inRaw, t);

        // ��ǥ �ӵ�
        float maxSpd = Input.GetKey(KeyCode.LeftShift) ? focusSpeed : speed;
        Vector2 desiredVel = _inSm * maxSpd;

        // ���� 2) �ӵ� ���� (����/����)
        float a = desiredVel.sqrMagnitude > 0.001f ? accel : decel;
        _vel = Vector2.MoveTowards(_vel, desiredVel, a * Time.deltaTime);

        // ���� ����
        if (_vel.sqrMagnitude < stopEpsilon * stopEpsilon && desiredVel == Vector2.zero)
            _vel = Vector2.zero;

        // ���� 3) ���� �̵�
        transform.position += (Vector3)(_vel * Time.deltaTime);

        // (����) ��� ����
        if (worldClamp.x < 900f)
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -worldClamp.x, worldClamp.x);
            p.y = Mathf.Clamp(p.y, -worldClamp.y, worldClamp.y);
            transform.position = p;
        }
    }
}
