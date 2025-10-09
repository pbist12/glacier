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
        Dialogue,
        Normal,
        Elite,
        Shop,
        Boss,
        Result
    }
    [SerializeField] private GameState state = GameState.Prologue;
    public GameState State { get => state; set => state = value; }
    #endregion

    [Header("Score / Pause")]
    [SerializeField] private int score;
    [SerializeField] private bool paused;
    public int Score { get => score; set => score = value; }
    public bool Paused { get => paused; set => paused = value; }

    [Header("Flow Tunables")]
    public int elitesToBoss = 2;           // 엘리트 페이즈 N번 완료 후 보스전 진입
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
    }

    void SetState(GameState s)
    {
        State = s;
        Debug.Log("State :" + s);
    }

    #region 몬스터 페이즈 변경
    public void StartNormalPhase()
    {
        paused = false;
        SetState(GameState.Normal);

        if (!stageManager.isEnd)
        {
            var stage = stageManager.stages[stageManager._stageIndex];
            spawner.LoadStage(stage);
        }

        spawner.EnableElite(false);
        spawner.EnableBoss(false);
        spawner.BeginNormalPhase();

        // "일반 전멸 → 엘리트 진입" 구독
        if (spawner != null)
        {
            spawner.OnNormalsCleared -= HandleNormalsCleared;
            spawner.OnNormalsCleared += HandleNormalsCleared;
        }
    }

    public void RequestElitePhase()
    {
        if (State != GameState.Normal) return;
        SetState(GameState.Elite);
        if (spawner)
        {
            spawner.EnableNormal(false);
            spawner.EnableElite(true);
            spawner.SpawnElitePack();
        }
    }

    public void BackToNormalAfterElite()
    {
        if (_eliteClears >= elitesToBoss)
            StartBossPhase();
        else
            StartNormalPhase();
    }

    public void StartBossPhase()
    {
        SetState(GameState.Boss);
        if (spawner)
        {
            spawner.EnableNormal(false);
            spawner.EnableElite(false);
            spawner.EnableBoss(true);
            spawner.BeginBossPhaseWithDelay();
        }
    }
    #endregion

    #region 몬스터 사망 이벤트
    public void OnEnemyKilled(bool isElite, int scoreGain)
    {
        // 이제 처치 수 기반 전환은 사용하지 않음: 점수만 처리
        score += scoreGain;
    }

    public void OnEliteUnitKilled(int scoreGain)
    {
        score += scoreGain;
        // 엘리트 전멸 여부는 스포너가 판단 후 OnEliteCleared() 호출
    }

    public void OnEliteCleared()
    {
        bool spawnShop = Random.value < shopPortalChance;
        _eliteClears++;
        Debug.Log($"spawnShop: {spawnShop}, spawner is null: {spawner == null}");

        if (spawnShop && spawner)
        {
            Debug.Log("Spawning shop portal and returning");
            spawner.SpawnShopPortal();
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
            _eliteClears = 0;
            SceneLoader.Instance.ReloadCurrent();
        }
    }
    #endregion

    // 일반 전멸 신호 → 엘리트 페이즈 진입
    private void HandleNormalsCleared()
    {
        if (State != GameState.Normal) return;
        RequestElitePhase();
    }

    #region 대화창
    public void StartDialogue()
    {
        SetState(GameState.Dialogue);
        DialogueService.Instance.Play(intro);
    }
    #endregion

    #region 상점
    public void StartShopPhase()
    {
        if (State == GameState.Shop) return;
        SetState(GameState.Shop);
    }

    public void ExitShop()
    {
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

    public void AddScore(int amount) => score += amount;
    public void TogglePause() { Paused = !Paused; }

    #region Bind
    void OnEnable()
    {
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
        const FindObjectsInactive Inactive = FindObjectsInactive.Include;

        spawner = spawner ? spawner : FindFirstObjectByType<EnemySpawner>(Inactive);
        shop = shop ? shop : FindFirstObjectByType<ShopManager>(Inactive);
        result = result ? result : FindFirstObjectByType<ResultScreen>(Inactive);
        vssample = vssample ? vssample : FindFirstObjectByType<VerticalScrollerSimple>(Inactive);
        stageManager = stageManager ? stageManager : FindFirstObjectByType<StageManager>(Inactive);

        if (!spawner) Debug.LogWarning("[GameRefBinder] EnemySpawner를 찾지 못했습니다.");
        if (!shop) Debug.LogWarning("[GameRefBinder] ShopManager를 찾지 못했습니다.");
        if (!result) Debug.LogWarning("[GameRefBinder] ResultScreen을 찾지 못했습니다.");
        if (!vssample) Debug.LogWarning("[GameRefBinder] VerticalScrollerSimple을 찾지 못했습니다.");
        if (!stageManager) Debug.LogWarning("[GameRefBinder] StageManager를 찾지 못했습니다.");
    }
    #endregion
}
