using UnityEngine;

[DisallowMultipleComponent]
public class BossMover : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // �̵���ų Transform (������ this.transform)

    [Header("Defaults")]
    public bool rotateZIn2D = true; // 2D ȸ��(Z��)

    void Reset()
    {
        target = transform;
    }

    public Transform T
    {
        get { return target ? target : transform; }
    }

    /// ��� ��ġ ����
    public void SetPosition(Vector3 worldPos)
    {
        T.position = worldPos;
    }

    /// ������ �̵�(����)
    public void AddOffset(Vector2 worldDelta)
    {
        T.position += (Vector3)worldDelta;
    }

    /// ���� ���� ��ġ ���� (0..1)
    public void LerpTo(Vector3 from, Vector3 to, float t)
    {
        T.position = Vector3.LerpUnclamped(from, to, t);
    }

    /// �ӵ� ��� �̵�(������ ����)
    public void MoveTowards(Vector3 targetPos, float maxSpeed)
    {
        float step = maxSpeed * Time.deltaTime;
        T.position = Vector3.MoveTowards(T.position, targetPos, step);
    }

    /// ���� ȸ�� (2D: Z�� ȸ��)
    public void FaceDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (rotateZIn2D)
            T.rotation = Quaternion.Euler(0, 0, z);
        else
            T.rotation = Quaternion.LookRotation(Vector3.forward, dir); // �ʿ�� ����
    }
}
