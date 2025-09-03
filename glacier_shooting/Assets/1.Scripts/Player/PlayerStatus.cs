using Unity.VisualScripting;
using UnityEngine;
using VFolders.Libs;

public class PlayerStatus : MonoBehaviour
{
    public PlayerData player;

    [SerializeField] private int playerHealth;
    [SerializeField] private int playerMaxHealth;

    [SerializeField] private int bomb;

    public int PlayerHealth 
    {
        get { return playerHealth; }
        set { playerHealth = value; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = playerMaxHealth;
    }

    #region 플레이어 공격받음
    private void OnDamaged()
    {
        playerHealth--;

        if (playerHealth <= 0)
        {
            //여기에 플레이어 사망 이벤트
            this.gameObject.Destroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnDamaged();
    }
    #endregion

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

}
