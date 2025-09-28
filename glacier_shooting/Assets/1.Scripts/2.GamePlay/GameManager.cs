using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region GameState
    public enum GameState
    {
        Prologue,
        Normal,      
        Elite,         
        Shop,         
        Boss,         
        Result        
    }
    // 런타임 상태
    [SerializeField] private GameState state = GameState.Prologue;
    public GameState State
    {
        get { return state; }
        set { state = value; }
    }
    #endregion

    [Header("Score / Pause")]
    [SerializeField] private int score;
    [SerializeField] private bool paused;

    public int Score
    {
        get { return score; }
        set { score = value; }
    }
    public bool Paused { get => paused; set { paused = value;} }

    [Header("Flow Tunables")]
    public int killsToElite = 25;          // 일반 페이즈에서 이 킬 수 달성 시 엘리트 페이즈로
    public int elitesToBoss = 2;           // 엘리트 페이즈 N번 완료 후 보스전 진입
    [SerializeField] private int _normalKills = 0;
    [SerializeField] private int _eliteClears = 0;

    public float shopPortalChance = 0.35f; // 엘리트 처치 후 포탈 등장 확률

    [Header("임시 다이얼로그 데이터")]
    public DialogueData intro;

    #region 연결 스크립트
    private EnemySpawner spawner;
    private ShopManager shop;
    private ResultScreen result;
    private VerticalScrollerSimple vssample;
    private StageManager stageManager;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        paused = true;
        SetState(GameState.Prologue);
        DialogueService.Instance.Play(intro);
    }

    void SetState(GameState s)
    {
        State = s;
        Debug.Log("State :" + s);
    }

    #region 몬스터 페이즈 변경
    public void StartNormalPhase()
    {
        _normalKills = 0;
        paused = false;

        SetState(GameState.Normal);

        if (!stageManager.isEnd)
        {
            var stage = stageManager.stages[stageManager._stageIndex]; // StageManager가 들고 있는 StageData
            spawner.LoadStage(stage);
        }

        // (2) 노말 페이즈 시작: 예산 기반 티커 ON
        spawner.EnableElite(false);
        spawner.EnableBoss(false);
        spawner.BeginNormalPhase();        // ✅ EnableNormal(true) 대신 이걸로 시작
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
            spawner.BeginBossPhaseWithDelay(); // 연출 후 보스 등장(스포너에 구현)
        }
    }
    #endregion

    #region 몬스터 사망 이벤트
    public void OnEnemyKilled(bool isElite, int scoreGain)
    {
        score += scoreGain;

        if (State == GameState.Normal && !isElite)
        {
            _normalKills++;
            if (_normalKills >= killsToElite) RequestElitePhase();
        }
    }
    public void OnEliteUnitKilled(int scoreGain)
    {
        score += scoreGain;
        // 엘리트 전멸 여부는 스포너가 판단해서 OnEliteCleared() 호출
    }
    public void OnEliteCleared()
    {
        bool spawnShop = Random.value < shopPortalChance;
        _eliteClears++;
        Debug.Log($"spawnShop: {spawnShop}, spawner is null: {spawner == null}");

        if (spawnShop && spawner)
        {
            Debug.Log("Spawning shop portal and returning");
            spawner.SpawnShopPortal(); // 포탈 프리팹 소환
            return;
        }

        if (!spawnShop)
        {
            BackToNormalAfterElite();
            Debug.Log("리턴 X 실행");
        }
    }
    public void OnBossKilled(int scoreGain)
    {
        score += scoreGain;
        OnBossDefeated();
    }
    public void OnBossDefeated()
    {
        stageManager.GoToNextStage();

        if (stageManager.isEnd)
        {
            ShowResult();
        }
        else
        {
            _normalKills = 0;
            _eliteClears = 0;
            SceneLoader.Instance.ReloadCurrent();
        }
    }
    #endregion

    #region 상점
    public void StartShopPhase()
    {
        if (State == GameState.Shop) return;
        SetState(GameState.Shop);
    }
    public void EnterShop()
    {
        if (shop) shop.Open();
    }
    public void ExitShop()
    {
        if (shop) shop.Close();
        Debug.Log($"spawner is null: {spawner == null}");
        spawner.DeSpawnShopPortal();
        paused = false;
        BackToNormalAfterElite();
    }
    #endregion

    #region 결과창
    public void ShowResult()
    {
        SetState(GameState.Result);
        Time.timeScale = 0f;
        if (result) result.Show(Score);
    }
    #endregion

    #region 시작 프롤로그
    public void StartPrologue()
    {
        paused = true;
        SetState(GameState.Prologue);
        StartCoroutine(WaitForPTime());
    }
    private IEnumerator WaitForPTime()
    {
        yield return new WaitForSeconds(3f);
        StartNormalPhase();
    }
    #endregion

    public void AddScore(int amount) => score += amount; // 호환용
    public void TogglePause() { Paused = !Paused; }

    #region Bind
    void OnEnable()
    {
        // 씬이 로드될 때마다 다시 바인딩
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindAll();
        StartNormalPhase();
    }

    private void BindAll()
    {
        // Inactive 오브젝트까지 포함해서 탐색 (UI가 꺼져 시작하는 경우 대비)
        const FindObjectsInactive Inactive = FindObjectsInactive.Include;

        spawner = spawner ? spawner : FindFirstObjectByType<EnemySpawner>(Inactive);
        shop = shop ? shop : FindFirstObjectByType<ShopManager>(Inactive);
        result = result ? result : FindFirstObjectByType<ResultScreen>(Inactive);
        vssample = vssample ? vssample : FindFirstObjectByType<VerticalScrollerSimple>(Inactive);
        stageManager = stageManager ? stageManager : FindFirstObjectByType<StageManager>(Inactive);

        // 선택: 필수 참조 누락 시 경고
        if (!spawner) Debug.LogWarning("[GameRefBinder] EnemySpawner를 찾지 못했습니다.");
        if (!shop) Debug.LogWarning("[GameRefBinder] ShopManager를 찾지 못했습니다.");
        if (!result) Debug.LogWarning("[GameRefBinder] ResultScreen을 찾지 못했습니다.");
        if (!vssample) Debug.LogWarning("[GameRefBinder] VerticalScrollerSimple을 찾지 못했습니다.");
        if (!stageManager) Debug.LogWarning("[GameRefBinder] StageManager를 찾지 못했습니다.");
    }
    #endregion
}
