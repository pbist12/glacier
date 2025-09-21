using UnityEngine;

/// <summary>
/// 플레이어의 아이템 마그넷(끌림) 설정을 관리.
/// - 아이템은 이 컴포넌트의 값을 참조해 이동/줍기 로직을 결정.
/// - 버프/장비/레벨업으로 값만 바꾸면 즉시 반영됨.
/// </summary>
[DisallowMultipleComponent]
public class PlayerItemMagnet : MonoBehaviour
{
    [Header("Ranges")]
    [Tooltip("이 범위 안에 들어오면 아이템이 끌려오기 시작")]
    public float followRange = 6f;

    [Tooltip("이 거리 이내면 즉시 줍기")]
    public float pickupRange = 0.7f;

    [Header("Speed")]
    [Tooltip("기본 끌림 속도(초당)")]
    public float basePullSpeed = 4f;

    [Tooltip("거리 기반 속도 보정(가까울수록 더 빨리 끌려오게 등) \nX: 정규화 거리(0=플레이어 근처, 1=followRange 경계)\nY: 속도 배수")]
    public AnimationCurve speedByDistance = AnimationCurve.Linear(0, 1, 1, 1);

    /// <summary>
    /// 현재 거리 dist와 최대 추적 범위 maxRange 기준으로 실제 끌림 속도 반환
    /// </summary>
    public float GetPullSpeed(float dist)
    {
        float t = Mathf.Clamp01(dist / Mathf.Max(0.0001f, followRange)); // 0(가까움)~1(멀어짐)
        float mult = speedByDistance.Evaluate(t); // 가까울수록 1 이상이 되도록 곡선을 커스텀해도 됨
        return Mathf.Max(0f, basePullSpeed * mult);
    }

    // --- 선택: 런타임 버프/디버프 헬퍼 ---
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
