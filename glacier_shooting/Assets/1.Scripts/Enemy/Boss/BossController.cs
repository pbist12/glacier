// File: BossController.cs
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BossHealth health;                 // ü��/��� �̺�Ʈ
    [SerializeField] private BossSpreadShooter spread;          // ��ä�� ź��
    [SerializeField] private BossHomingShooter homing;          // ����ź

    [Header("Move")]
    public bool enableMove = false;
    public float moveSpeed = 2f;
    public float moveRange = 2.5f;

    [Header("Phase")]
    [Tooltip("���� ������")]
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

        // �ʱ� ������ ����
        ApplyPhase(phase);

        // ����ź ����(������ 1������ ����)
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
        while (phase == 1) // ��: ������1������ �ֱ��� �߻�
        {
            if (homing) homing.FireOnce();
            yield return new WaitForSeconds(homing ? homing.coolDown : 2f);
        }
    }

    void HandleDeath()
    {
        // ����/��� ��
        Debug.Log("[BossController] Boss Defeated!");
        Destroy(gameObject);
    }
}
