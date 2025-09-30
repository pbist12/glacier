using UnityEngine;

/// <summary>
/// �÷��̾��� ������ ���׳�(����) ������ ����.
/// - �������� �� ������Ʈ�� ���� ������ �̵�/�ݱ� ������ ����.
/// - ����/���/���������� ���� �ٲٸ� ��� �ݿ���.
/// </summary>
[DisallowMultipleComponent]
public class PlayerItemMagnet : MonoBehaviour
{
    [Header("Ranges")]
    [Tooltip("�� ���� �ȿ� ������ �������� �������� ����")]
    public float followRange = 6f;

    [Tooltip("�� �Ÿ� �̳��� ��� �ݱ�")]
    public float pickupRange = 0.7f;

    [Header("Speed")]
    [Tooltip("�⺻ ���� �ӵ�(�ʴ�)")]
    public float basePullSpeed = 4f;

    [Tooltip("�Ÿ� ��� �ӵ� ����(�������� �� ���� �������� ��) \nX: ����ȭ �Ÿ�(0=�÷��̾� ��ó, 1=followRange ���)\nY: �ӵ� ���")]
    public AnimationCurve speedByDistance = AnimationCurve.Linear(0, 1, 1, 1);

    /// <summary>
    /// ���� �Ÿ� dist�� �ִ� ���� ���� maxRange �������� ���� ���� �ӵ� ��ȯ
    /// </summary>
    public float GetPullSpeed(float dist)
    {
        float t = Mathf.Clamp01(dist / Mathf.Max(0.0001f, followRange)); // 0(�����)~1(�־���)
        float mult = speedByDistance.Evaluate(t); // �������� 1 �̻��� �ǵ��� ��� Ŀ�����ص� ��
        return Mathf.Max(0f, basePullSpeed * mult);
    }

    // --- ����: ��Ÿ�� ����/����� ���� ---
    public void AddTemporaryFollowRange(float add, float duration)
        => StartCoroutine(CoTempModify(val => followRange += val, add, duration));

    public void AddTemporaryPickupRange(float add, float duration)
        => StartCoroutine(CoTempModify(val => pickupRange += val, add, duration));

    public void AddTemporaryPullSpeed(float add, float duration)
        => StartCoroutine(CoTempModify(val => basePullSpeed += val, add, duration));

    System.Collections.IEnumerator CoTempModify(System.Action<float> apply, float add, float duration)
    {
        apply(add);
        yield return new WaitForSeconds(duration);
        apply(-add);
    }
}
