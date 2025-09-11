using DG.Tweening.Core.Easing;
using Unity.VisualScripting;
using UnityEngine;
using VFolders.Libs;

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

    public int PlayerHealth 
    {
        get { return playerHealth; }
        set { playerHealth = value; }
    }

    void Awake()
    {
        // 이미 인스턴스가 있다면 자신을 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // 인스턴스 등록
        Instance = this;

        player = PlayerData.Instance.characterData;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetStat();
        playerHealth = playerMaxHealth;
    }

    #region 플레이어 공격받음
    private void OnDamaged()
    {
        playerHealth--;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            OnDamaged();
        }
    }
    #endregion

    public void SetStat()
    {
        playerShoot.fireRate = player.fireRate + addFireRate;
        playerShoot.bulletSpeed = player.bulletSpeed + addBulletSpeed;
        playerShoot.bulletLifetime = player.bulletLifetime + addBulletLifeTime;

        playerMaxHealth = player.maxLife;

        controller.speed = player.moveSpeed;
        controller.detailSpeed = player.focusSpeed;
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

}
