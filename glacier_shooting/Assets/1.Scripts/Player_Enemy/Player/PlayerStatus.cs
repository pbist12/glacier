using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus Instance { get; private set; }
    public CharacterData player;

    [SerializeField] private PlayerController controller;
    [SerializeField] private PlayerShoot playerShoot;

    [SerializeField] private int playerHealth;
    [SerializeField] private int playerMaxHealth;

    [SerializeField] private int bomb;

    [SerializeField] private float addFireRate;
    [SerializeField] private float addBulletSpeed;
    [SerializeField] private float addBulletLifeTime;
    [SerializeField] private float addBulletDamage;

    [Header("Collision Debug")]
    public float radius = 0.25f;
    public bool drawDebug = true;

    public int PlayerHealth
    {
        get { return playerHealth; }
        set { playerHealth = value; }
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
        SetStat();
        playerHealth = playerMaxHealth;
    }

    #region 플레이어 공격받음
    public void OnDamaged()
    {
        playerHealth--;
    }
    #endregion

    public void SetStat()
    {
        playerShoot.fireRate = player.fireRate + addFireRate;
        playerShoot.bulletSpeed = player.bulletSpeed + addBulletSpeed;
        playerShoot.bulletLifetime = player.bulletLifetime + addBulletLifeTime;
        playerShoot.bulletDamage = player.damage + addBulletDamage;

        playerMaxHealth = player.maxLife;

        controller.speed = player.moveSpeed;
        controller.focusSpeed = player.focusSpeed;
    }

    public void AddBome()
    {
        bomb++;
    }

    public void Addlife()
    {
        if (playerHealth < playerMaxHealth)
        {
            playerHealth++;
        }
    }

    public void AddStat(RelicData relic)
    {
        switch (relic.relicType)
        {
            case RelicType.Attack:
                addFireRate += relic.power;
                addBulletSpeed += relic.power;
                addBulletLifeTime += relic.power;
                SetStat();
                break;
        }
    }

    // ===== 디버그 표시 =====
    void OnDrawGizmos()
    {
        if (!drawDebug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);

#if UNITY_EDITOR
        // 체력/폭탄 수를 텍스트로 표시
        Handles.color = Color.white;
        string statusText = $"HP: {playerHealth}/{playerMaxHealth}\nBomb: {bomb}";
        Handles.Label(transform.position + Vector3.up * 0.5f, statusText);
#endif
    }
}
