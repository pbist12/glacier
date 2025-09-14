using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public PlayerStatus status;

    public TextMeshProUGUI playerHealthText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerHealthText.text = "Player Health : " + status.PlayerHealth.ToString();
    }
}
