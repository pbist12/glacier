using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Prologue,
        Normal,        // 일반 몬스터 페이즈
        Elite,         // 엘리트 처리 페이즈(한 번에 1~N마리 스폰)
        Shop,          // 상점 UI 오픈(시간 정지/전투 정지)
        Boss,          // 스토리 연출 후 보스전
        FinalBoss,     // (선택) 최종 보스
        Result,        // 스코어보드/최종 정산
        GameOver       // 플레이어 라이프 0에서의 종료
    }

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI stateText; // 디버그용

    [Header("Score / Pause")]
    [SerializeField] private int score;
    [SerializeField] private bool paused;
    public int Score { get => score; set { score = value; if (scoreText) scoreText.text = $"Score : {score}"; } }
    public bool Paused { get => paused; set { paused = value;} }

    [Header("Flow Tunables")]
    public int killsToElite = 25;          // 일반 페이즈에서 이 킬 수 달성 시 엘리트 페이즈로
    public float shopPortalChance = 0.35f; // 엘리트 처치 후 포탈 등장 확률
    public int elitesToBoss = 2;           // 엘리트 페이즈 N번 완료 후 보스전 진입
    public bool useFinalBoss = false;      // 최종 보스 사용 여부

    [Header("Refs")]
    public EnemySpawner spawner;     // 씬의 스포너 할당
    public ShopManager shop;         // 상점 매니저(아래 스텁)
    public ResultScreen result;      // 결과 화면(아래 스텁)
    public VerticalScrollerSimple vssample;

    [Header("임시 다이얼로그 데이터")]
    public DialogueData intro;

    // 런타임 상태
    [SerializeField] private GameState state = GameState.Prologue;
    public GameState State
    {
        get { return state; }
        set { state = value; }
    }
    [SerializeField] private int _normalKills = 0;    // Normal 페이즈 누적 처치수
    [SerializeField] private int _eliteClears = 0;    // 엘리트 페이즈 완료 횟수

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetState(GameState.Prologue); // 스토리 프롤로그 재생(컷신/자막 등)
        DialogueService.Instance.Play(intro);
    }

    void SetState(GameState s)
    {
        State = s;
        if (stateText) stateText.text = $"State : {State}";
    }

    #region 몬스터 페이즈 변경
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
        // 일반 페이즈에서 호출: 목표 처치 수 달성 시
        if (State != GameState.Normal) return;
        SetState(GameState.Elite);
        if (spawner)
        {
            spawner.EnableNormal(false);
            spawner.EnableElite(true);
            spawner.SpawnElitePack();      // 엘리트 1~N마리 스폰 (스포너에 구현)
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
            spawner.ArmBossCountdownOrSpawn(); // 연출 후 보스 등장(스포너에 구현)
        }
    }
    #endregion

    #region 몬스터 사망 이벤트
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
        // 엘리트 전멸 여부는 스포너가 판단해서 OnEliteCleared() 호출
    }
    public void OnEliteCleared()
    {
        bool spawnShop = Random.value < shopPortalChance;
        _eliteClears++;

        if (spawnShop && spawner)
        {
            spawner.SpawnShopPortal(); // 포탈 프리팹 소환
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

    #region 상점
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
        if (result) result.Show(Score); // 같은 결과창을 재활용
        Debug.Log("Game Over");
    }
    public void OnPlayerLifeZero()
    {
        GameOver();
    }
    public void AddScore(int amount) => Score += amount; // 호환용
    public void TogglePause() { Paused = !Paused; }
}
