using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerStatus : MonoBehaviour, IPlayerStats
{
    public static PlayerStatus Instance { get; private set; }
    public CharacterData player;

    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PlayerInventory playerInventory; // �� �߰�: �κ��丮 ����

    [SerializeField] private int playerHealth;
    [SerializeField] private int playerMaxHealth;
    [SerializeField] private int bomb;

    // === ����(mod add) ===
    [SerializeField] private float addFireRate;
    [SerializeField] private float addBulletSpeed;
    [SerializeField] private float addBulletLifeTime;
    [SerializeField] private float addBulletDamage;
    [SerializeField] private float addMoveSpeed;
    [SerializeField] private float addFocusSpeed;
    [SerializeField] private int addMaxHP;

    // === ����(mod percent, 0.15 = +15%) ===
    private float mulFireRate;
    private float mulBulletSpeed;
    private float mulBulletLifeTime;
    private float mulBulletDamage;
    private float mulMoveSpeed;
    private float mulFocusSpeed;
    private float mulMaxHP;

    public bool invincible = false;

    [Header("Collision Debug")]
    public float radius = 0.25f;
    public bool drawDebug = true;

    public int PlayerHealth
    {
        get => playerHealth;
        set => playerHealth = value;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (PlayerData.Instance != null)
            player = PlayerData.Instance.characterData;
    }

    void Start()
    {
        ResetAllModifiers();  // �� ���� �� ������̾� �ʱ�ȭ
        SetStat();
        playerHealth = playerMaxHealth;
    }

    #region ���� ó��
    public void OnDamaged()
    {
        if (invincible) return;
        playerHealth = Mathf.Max(0, playerHealth - 1);
        // �ʿ� �� ��� ó�� ��
    }
    #endregion

    /// <summary>
    /// �⺻ ����(player) + ����(add) + ����(mul)�� ����ġ ���
    /// </summary>
    public void SetStat()
    {
        // FireRate
        playerShoot.fireRate = (player.fireRate + addFireRate) * (1f + mulFireRate);

        // Bullet
        playerShoot.bulletSpeed = (player.bulletSpeed + addBulletSpeed) * (1f + mulBulletSpeed);
        playerShoot.bulletLifetime = (player.bulletLifetime + addBulletLifeTime) * (1f + mulBulletLifeTime);
        playerShoot.bulletDamage = (player.damage + addBulletDamage) * (1f + mulBulletDamage);

        // HP
        int baseMax = player.maxLife + addMaxHP;
        baseMax = Mathf.Max(1, Mathf.RoundToInt(baseMax * (1f + mulMaxHP)));
        playerMaxHealth = baseMax;
        playerHealth = Mathf.Clamp(playerHealth, 0, playerMaxHealth);

        // Move
        controller.speed = (player.moveSpeed + addMoveSpeed) * (1f + mulMoveSpeed);
        controller.focusSpeed = (player.focusSpeed + addFocusSpeed) * (1f + mulFocusSpeed);
    }

    public void AddBomb() => bomb++;

    public void AddLife()
    {
        if (playerHealth < playerMaxHealth)
            playerHealth++;
    }

    /// <summary>
    /// ������ ���� (OnUse/Passive/��� ���� �� ��)
    /// </summary>
    public void ApplyItem(ItemData item)
    {
        var ctx = new ItemContext(
            owner: gameObject,
            inventory: playerInventory,
            stats: this, // IPlayerStats ���� �� StatModifierEffect ���� �������� ����
            logger: Debug.Log
        );

        foreach (var effect in item.effects)
            effect?.Apply(ctx);

        SetStat();
    }

    /// <summary>
    /// ������ ����(��� ����/�нú� ���� ��)
    /// </summary>
    public void RemoveItem(ItemData item)
    {
        var ctx = new ItemContext(
            owner: gameObject,
            inventory: playerInventory,
            stats: this,
            logger: Debug.Log
        );

        // ȿ�� ���� (revertOnRemove=true �� ȿ������ ����)
        for (int i = item.effects.Count - 1; i >= 0; i--)
            item.effects[i]?.Remove(ctx);

        SetStat();
    }

    // ===== IPlayerStats ������ =====
    public void AddModifier(StatType stat, int flatDelta, float percentDelta)
    {
        switch (stat)
        {
            case StatType.FireRate:
                addFireRate += flatDelta;
                mulFireRate += percentDelta;
                break;
            case StatType.BulletSpeed:
                addBulletSpeed += flatDelta;
                mulBulletSpeed += percentDelta;
                break;
            case StatType.BulletLifetime:
                addBulletLifeTime += flatDelta;
                mulBulletLifeTime += percentDelta;
                break;
            case StatType.BulletDamage:
                addBulletDamage += flatDelta;
                mulBulletDamage += percentDelta;
                break;
            case StatType.MoveSpeed:
                addMoveSpeed += flatDelta;
                mulMoveSpeed += percentDelta;
                break;
            case StatType.FocusSpeed:
                addFocusSpeed += flatDelta;
                mulFocusSpeed += percentDelta;
                break;
            case StatType.MaxHP:
                addMaxHP += flatDelta;
                mulMaxHP += percentDelta;
                // HP ������ SetStat����
                break;
            default:
                Debug.LogWarning($"[PlayerStatus] Unhandled StatType: {stat}");
                break;
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        playerHealth = Mathf.Clamp(playerHealth + amount, 0, playerMaxHealth);
    }

    /// <summary>��� ������̾� �ʱ�ȭ</summary>
    public void ResetAllModifiers()
    {
        addFireRate = addBulletSpeed = addBulletLifeTime = addBulletDamage = 0f;
        addMoveSpeed = addFocusSpeed = 0f;
        addMaxHP = 0;

        mulFireRate = mulBulletSpeed = mulBulletLifeTime = mulBulletDamage = 0f;
        mulMoveSpeed = mulFocusSpeed = 0f;
        mulMaxHP = 0f;
    }

    // ===== ����� ǥ�� =====
    void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);

#if UNITY_EDITOR
        Handles.color = Color.white;
        string statusText = $"HP: {playerHealth}/{playerMaxHealth}\nBomb: {bomb}";
        Handles.Label(transform.position + Vector3.up * 0.5f, statusText);
#endif
    }
}
