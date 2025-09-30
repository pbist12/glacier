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
            Destroy(gameObject); // �ߺ� ����
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
