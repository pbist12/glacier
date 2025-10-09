using UnityEngine;

[DisallowMultipleComponent]
public class BossMover : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // 이동시킬 Transform (없으면 this.transform)

    [Header("Defaults")]
    public bool rotateZIn2D = true; // 2D 회전(Z축)

    void Reset()
    {
        target = transform;
    }

    public Transform T
    {
        get { return target ? target : transform; }
    }

    /// 즉시 위치 세팅
    public void SetPosition(Vector3 worldPos)
    {
        T.position = worldPos;
    }

    /// 오프셋 이동(월드)
    public void AddOffset(Vector2 worldDelta)
    {
        T.position += (Vector3)worldDelta;
    }

    /// 선형 보간 위치 세팅 (0..1)
    public void LerpTo(Vector3 from, Vector3 to, float t)
    {
        T.position = Vector3.LerpUnclamped(from, to, t);
    }

    /// 속도 기반 이동(프레임 독립)
    public void MoveTowards(Vector3 targetPos, float maxSpeed)
    {
        float step = maxSpeed * Time.deltaTime;
        T.position = Vector3.MoveTowards(T.position, targetPos, step);
    }

    /// 방향 회전 (2D: Z축 회전)
    public void FaceDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (rotateZIn2D)
            T.rotation = Quaternion.Euler(0, 0, z);
        else
            T.rotation = Quaternion.LookRotation(Vector3.forward, dir); // 필요시 수정
    }
}
