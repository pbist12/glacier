using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Prologue,
        Normal,        // �Ϲ� ���� ������
        Elite,         // ����Ʈ ó�� ������(�� ���� 1~N���� ����)
        Shop,          // ���� UI ����(�ð� ����/���� ����)
        Boss,          // ���丮 ���� �� ������
        FinalBoss,     // (����) ���� ����
        Result,        // ���ھ��/���� ����
        GameOver       // �÷��̾� ������ 0������ ����
    }

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI stateText; // ����׿�

    [Header("Score / Pause")]
    [SerializeField] private int score;
    [SerializeField] private bool paused;
    public int Score { get => score; set { score = value; if (scoreText) scoreText.text = $"Score : {score}"; } }
    public bool Paused { get => paused; set { paused = value;} }

    [Header("Flow Tunables")]
    public int killsToElite = 25;          // �Ϲ� ������� �� ų �� �޼� �� ����Ʈ �������
    public float shopPortalChance = 0.35f; // ����Ʈ óġ �� ��Ż ���� Ȯ��
    public int elitesToBoss = 2;           // ����Ʈ ������ N�� �Ϸ� �� ������ ����
    public bool useFinalBoss = false;      // ���� ���� ��� ����

    [Header("Refs")]
    public EnemySpawner spawner;     // ���� ������ �Ҵ�
    public ShopManager shop;         // ���� �Ŵ���(�Ʒ� ����)
    public ResultScreen result;      // ��� ȭ��(�Ʒ� ����)
    public VerticalScrollerSimple vssample;

    [Header("�ӽ� ���̾�α� ������")]
    public DialogueData intro;

    // ��Ÿ�� ����
    [SerializeField] private GameState state = GameState.Prologue;
    public GameState State
    {
        get { return state; }
        set { state = value; }
    }
    [SerializeField] private int _normalKills = 0;    // Normal ������ ���� óġ��
    [SerializeField] private int _eliteClears = 0;    // ����Ʈ ������ �Ϸ� Ƚ��

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetState(GameState.Prologue); // ���丮 ���ѷα� ���(�ƽ�/�ڸ� ��)
        DialogueService.Instance.Play(intro);
    }

    void SetState(GameState s)
    {
        State = s;
        if (stateText) stateText.text = $"State : {State}";
    }

    #region ���� ������ ����
    public void StartNormalPhase()
    {
        _normalKills = 0;
        SetState(GameState.Normal);
        if (spawner)
        {
            spawner.EnableNormal(true);
            spawner.EnableBoss(false);
            spawner.EnableElite(false);
        }
    }
    public void RequestElitePhase()
    {
        // �Ϲ� ������� ȣ��: ��ǥ óġ �� �޼� ��
        if (State != GameState.Normal) return;
        SetState(GameState.Elite);
        if (spawner)
        {
            spawner.EnableNormal(false);
            spawner.EnableElite(true);
            spawner.SpawnElitePack();      // ����Ʈ 1~N���� ���� (�����ʿ� ����)
        }
    }
    public void BackToNormalAfterElite()
    {
        if (_eliteClears >= elitesToBoss)
        {
            StartBossPhase();
        }
        else
        {
            StartNormalPhase();
        }
    }
    public void StartBossPhase()
    {
        SetState(GameState.Boss);
        if (spawner)
        {
            spawner.EnableNormal(false);
            spawner.EnableElite(false);
            spawner.EnableBoss(true);
            spawner.ArmBossCountdownOrSpawn(); // ���� �� ���� ����(�����ʿ� ����)
        }
    }
    #endregion

    #region ���� ��� �̺�Ʈ
    public void OnEnemyKilled(bool isElite, int scoreGain)
    {
        Score += scoreGain;

        if (State == GameState.Normal && !isElite)
        {
            _normalKills++;
            if (_normalKills >= killsToElite) RequestElitePhase();
        }
    }
    public void OnEliteUnitKilled(int scoreGain)
    {
        Score += scoreGain;
        // ����Ʈ ���� ���δ� �����ʰ� �Ǵ��ؼ� OnEliteCleared() ȣ��
    }
    public void OnEliteCleared()
    {
        bool spawnShop = Random.value < shopPortalChance;
        _eliteClears++;

        if (spawnShop && spawner)
        {
            spawner.SpawnShopPortal(); // ��Ż ������ ��ȯ
        }
        else
        {
            BackToNormalAfterElite();
        }
    }
    public void OnBossKilled(int scoreGain)
    {
        Score += scoreGain;
        OnBossDefeated();
    }
    public void OnBossDefeated()
    {
        if (useFinalBoss)
        {
            //SetState(GameState.FinalBoss);
            if (spawner)
            {
                StartNormalPhase();
            }
        }
        else
        {
            ShowResult();
        }
    }
    public void OnFinalBossDefeated()
    {
        ShowResult();
    }
    #endregion

    #region ����
    public void EnterShop()
    {
        if (State == GameState.Shop) return;
        SetState(GameState.Shop);     
        if (shop) shop.Open();
    }
    public void ExitShop()
    {
        if (shop) shop.Close();
        spawner.DeSpawnShopPortal();
        BackToNormalAfterElite();
    }
    #endregion

    public void ShowResult()
    {
        SetState(GameState.Result);
        Time.timeScale = 0f;
        if (result) result.Show(Score);
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
        Time.timeScale = 0f;
        if (result) result.Show(Score); // ���� ���â�� ��Ȱ��
        Debug.Log("Game Over");
    }
    public void OnPlayerLifeZero()
    {
        GameOver();
    }
    public void AddScore(int amount) => Score += amount; // ȣȯ��
    public void TogglePause() { Paused = !Paused; }
}
