using UnityEngine;

public class GameStatus : MonoBehaviour
{
    public static GameStatus Instance { get; private set; }

    public CharacterData characterData;
    public float playerAllMoney;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
