using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerStatus : MonoBehaviour, IPlayerStats
{
    public static PlayerStatus Instance { get; private set; }

    private SpriteRenderer playerSprite;
    public CharacterData player;

    private PlayerUI playerUI;
    private PlayerController controller;
    private PlayerShoot playerShoot;
    private PlayerInventory playerInventory;
    private PlayerItemMagnet playerMagenet;

    [SerializeField] private int playerHealth;
    [SerializeField] private int playerMaxHealth;
    [SerializeField] private float playerMana;
    [SerializeField] private float playerMaxMana;

    #region Add Stat
    // === 가산(mod add) ===
    private float addFireRate;
    private float addBulletSize;
    private float addBulletSpeed;
    private float addBulletLifeTime;
    private float addBulletDamage;
    private float addMoveSpeed;
    private float addFocusSpeed;
    private int addMaxHP;
    private int addMaxMana;
    private float addMagnetRange;
    #endregion

    #region Mul Stat
    // === 배율(mod percent, 0.15 = +15%) ===
    private float mulFireRate;
    private float mulBulletSize;
    private float mulBulletSpeed;
    private float mulBulletLifeTime;
    private float mulBulletDamage;
    private float mulMoveSpeed;
    private float mulFocusSpeed;
    private float mulMaxHP;
    private float mulMaxMana;
    #endregion

    [Header("무적")]
    public bool invincible = false;
    public float invincibleDuration;
    [SerializeField] private float flashInterval = 0.1f;   // 깜빡임 주기

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

        if (GameStatus.Instance != null)
        {
            player = GameStatus.Instance.characterData;
        }

        playerSprite = GetComponent<SpriteRenderer>();

        controller = GetComponent<PlayerController>();
        playerShoot = GetComponent<PlayerShoot>();
        playerMagenet = GetComponent<PlayerItemMagnet>();
        playerInventory = FindFirstObjectByType<PlayerInventory>();

        playerUI = FindFirstObjectByType<PlayerUI>();
        playerUI.RefreshStats(controller.speed, playerMaxHealth, playerShoot.bulletDamage, playerShoot.fireRate, 10);
    }

    void Start()
    {
        ResetAllModifiers();  // ★ 시작 시 모디파이어 초기화
        SetStat();
        playerHealth = playerMaxHealth;
    }

    #region 피해 처리
    public void OnDamaged()
    {
        if (invincible) return;
        playerHealth = Mathf.Max(0, playerHealth - 1);

        if (playerHealth <= 0)
        {
            GameManager.Instance.ShowResult();
        }
        else
        {
            StartCoroutine(SetInvincibleForSeconds(invincibleDuration));
        }
        // 필요 시 사망 처리 등
    }

    private IEnumerator SetInvincibleForSeconds(float duration)
    {
        invincible = true;

        float elapsed = 0f;
        while (elapsed < invincibleDuration)
        {
            // 스프라이트 깜빡이기
            playerSprite.enabled = !playerSprite.enabled;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // 끝날 때 스프라이트는 다시 켜줌
        playerSprite.enabled = true;
        invincible = false;
    }

    #endregion

    /// <summary>
    /// 기본 스탯(player) + 가산(add) + 배율(mul)로 최종치 계산
    /// </summary>
    public void SetStat()
    {
        // FireRate
        playerShoot.fireRate = (player.fireRate + addFireRate) * (1f + mulFireRate);

        // Bullet
        playerShoot.bulletSpeed = (player.bulletSpeed + addBulletSpeed) * (1f + mulBulletSpeed);
        playerShoot.bulletSize = (player.bulletSize + addBulletSize) * (1f + mulBulletSize);
        playerShoot.bulletLifetime = (player.bulletLifetime + addBulletLifeTime) * (1f + mulBulletLifeTime);
        playerShoot.bulletDamage = (player.damage + addBulletDamage) * (1f + mulBulletDamage);

        // Magnet
        playerMagenet.followRange = (player.magnetRange + addMagnetRange);

        // HP
        int baseMax = player.maxLife + addMaxHP;
        baseMax = Mathf.Max(1, Mathf.RoundToInt(baseMax * (1f + mulMaxHP)));
        playerMaxHealth = baseMax;
        playerHealth = Mathf.Clamp(playerHealth, 0, playerMaxHealth);

        // Move
        controller.speed = (player.moveSpeed + addMoveSpeed) * (1f + mulMoveSpeed);
        controller.focusSpeed = (player.focusSpeed + addFocusSpeed) * (1f + mulFocusSpeed);

        playerUI.RefreshStats(controller.speed, playerMaxHealth, playerShoot.bulletDamage, playerShoot.fireRate, 10);
    }

    // ===== IPlayerStats 구현부 =====
    public void AddModifier(StatType stat, int flatDelta, float percentDelta)
    {
        switch (stat)
        {
            case StatType.FireRate:
                addFireRate += flatDelta;
                mulFireRate += percentDelta;
                break;
            case StatType.BulletSize:
                addBulletSize += flatDelta;
                mulBulletSize += percentDelta;
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
                // HP 재계산은 SetStat에서
                break;
            case StatType.MagnetRange:
                addMagnetRange += flatDelta;
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

    /// <summary>모든 모디파이어 초기화</summary>
    public void ResetAllModifiers()
    {
        addFireRate = addBulletSpeed = addBulletLifeTime = addBulletDamage = 0f;
        addMoveSpeed = addFocusSpeed = 0f;
        addMaxHP = 0;

        mulFireRate = mulBulletSpeed = mulBulletLifeTime = mulBulletDamage = 0f;
        mulMoveSpeed = mulFocusSpeed = 0f;
        mulMaxHP = 0f;
    }

    // ===== 디버그 표시 =====
    void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
