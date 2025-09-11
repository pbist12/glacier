// File: BossController.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BossHealth health;                 // 체력/사망 이벤트
    [SerializeField] private BossSpreadShooter spread;          // 부채꼴 탄막
    [SerializeField] private BossHomingShooter homing;          // 유도탄

    [Header("Move")]
    public bool enableMove = false;
    public float moveSpeed = 2f;
    public float moveRange = 2.5f;

    [Header("Phase")]
    [Tooltip("시작 페이즈")]
    [SerializeField] private int phase = 1;

    void Reset()
    {
        health = GetComponent<BossHealth>();
        spread = GetComponent<BossSpreadShooter>();
        homing = GetComponent<BossHomingShooter>();
    }

    void OnEnable()
    {
        if (!health) health = GetComponent<BossHealth>();
        if (health)
        {
            health.onDeath += HandleDeath;
            health.onHpChanged += HandleHpChanged;
        }

        // 초기 페이즈 세팅
        ApplyPhase(phase);

        // 유도탄 루프(페이즈 1에서만 예시)
        if (homing) StartCoroutine(HomingLoop());
    }

    void OnDisable()
    {
        if (health)
        {
            health.onDeath -= HandleDeath;
            health.onHpChanged -= HandleHpChanged;
        }
    }

    void Update()
    {
        if (enableMove)
        {
            float x = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2f) - moveRange;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }
    }

    void HandleHpChanged(float hp, float max)
    {
        float r = hp / Mathf.Max(1f, max);
        if (phase == 1 && r < 0.7f) ApplyPhase(2);
        else if (phase == 2 && r < 0.3f) ApplyPhase(3);
    }

    void ApplyPhase(int newPhase)
    {
        phase = newPhase;

        switch (phase)
        {
            case 1:
                spread.isFire = true;
                break;
            case 2:
                spread.isFire = false;
                break;
            case 3:
                break;
        }
    }

    IEnumerator HomingLoop()
    {
        while (phase == 1) // 예: 페이즈1에서만 주기적 발사
        {
            if (homing) homing.FireOnce();
            yield return new WaitForSeconds(homing ? homing.coolDown : 2f);
        }
    }

    void HandleDeath()
    {
        // 연출/드랍 등
        Debug.Log("[BossController] Boss Defeated!");
        Destroy(gameObject);
    }
}
