using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player / UI")]
    public TextMeshProUGUI scoreText;
    public int Score { get; private set; }
    public bool Paused { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // --- 점수 관리 ---
    public void AddScore(int amount)
    {
        Score += amount;
        scoreText.text = "Score : " + Score;
    }

    // --- 라이프 관리 ---
    public void OnPlayerDied()
    {
        if (PlayerStatus.Instance.PlayerHealth <= 0)
        {
            GameOver();
        }
        else
        {
            // TODO: 리스폰 로직
        }
    }

    // --- 일시정지 ---
    public void TogglePause()
    {
        Paused = !Paused;
        Time.timeScale = Paused ? 0f : 1f;
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0f;
    }
}
