using UnityEngine;
using UnityEngine.Events;

public class Bomb : MonoBehaviour
{
    [Header("Refs")]
    public BulletPoolHub hub;                 // ����θ� �ڵ� Ž��
    public PlayerInventory playerInventory;

    [Header("Bomb Options")]
    public bool includePlayerBullets = false; // true�� �÷��̾� ź�� ����
    public bool useRadius = false;            // true�� �ݰ� ���� ���
    public float radius = 6f;                 // �ݰ� ũ��
    public Transform center;                  // ������(���� ���� ��ġ)

    [Header("Limits")]
    public float cooldown = 1.5f;             // ��ź ���� ���(��)

    [Header("Events")]
    public UnityEvent onBombFired;            // ���� ��(�÷���/���� ��)

    float _nextUseTime = 0f;

    void Awake()
    {
        if (!hub) hub = FindFirstObjectByType<BulletPoolHub>();
        if (!playerInventory) playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (!center) center = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            TryUseBomb();
    }

    public bool TryUseBomb()
    {
        if (!hub) { Debug.LogWarning("[BombTrigger] hub�� �����ϴ�."); return false; }
        if (Time.time < _nextUseTime) return false;
        if (playerInventory.bomb <= 0) return false;

        if (useRadius)
        {
            Vector2 c = center ? (Vector2)center.position : Vector2.zero;
            // �÷��̾� ź ���� ����: b.poolKey != BulletPoolKey.Player
            hub.BombClearInRadius(c, radius, includePlayerBullets ? null : (b => b.poolKey != BulletPoolKey.Player));
        }
        else
        {
            hub.BombClearAll(includePlayerBullets);
        }

        playerInventory.bomb--;
        _nextUseTime = Time.time + cooldown;
        onBombFired?.Invoke();
        return true;
    }
}
