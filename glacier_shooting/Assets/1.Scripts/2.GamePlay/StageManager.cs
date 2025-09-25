using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stages (순서대로 진행)")]
    public StageData[] stages;

    // 내부 상태
    [SerializeField] public int _stageIndex;      // 현재 스테이지 인덱스
    [SerializeField] private BossAsset _stageBoss; // 이번 스테이지에 선택된 보스
    public bool isEnd;

    // 읽기 전용 프로퍼티
    public int CurrentStageIndex => _stageIndex;
    public bool IsCampaignDone => stages == null || _stageIndex >= (stages?.Length ?? 0);
    public bool BossSelected => _stageBoss != null;
    public BossAsset SelectedBoss => _stageBoss;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // 다음 스테이지로 이동. 성공 시 true, 캠페인 종료 시 false
    public void GoToNextStage()
    {
        _stageIndex++;
        if (_stageIndex == stages.Length)
        {
            isEnd = true;
            return;
        }
        else
        {
            SelectRandomBossForCurrentStage();
        }
    }

    // ───── 외부에서 호출하는 간단 API ─────

    // 1) 잡몹 스폰 정보 가져가기 → Spawner가 알아서 스폰
    public SpawnGroup[] GetCurrentStageMobs()
    {
        var s = stages[_stageIndex];
        if (s == null || s.waves.groups == null) return System.Array.Empty<SpawnGroup>();
        return s.waves.groups.ToArray();
    }

    // 2) 엘리트 프리팹 목록(필요 시)
    public GameObject[] GetCurrentStageElites()
    {
        var s = stages[_stageIndex];
        if (s == null || s.elites == null || s.elites.Count == 0) return System.Array.Empty<GameObject>();
        var arr = new GameObject[s.elites.Count];
        for (int i = 0; i < s.elites.Count; i++) arr[i] = s.elites[i]?.prefab;
        return arr;
    }

    // 이번 스테이지의 보스 프리팹 얻기
    public GameObject GetSelectedBossPrefab;

    // ─────────────────────────────────────────────────────────────

    private void SelectRandomBossForCurrentStage()
    {
        _stageBoss = null;
        var s = stages[_stageIndex];
        if (s == null || s.bosses == null || s.bosses.Count == 0) return;
        _stageBoss = s.bosses[Random.Range(0, s.bosses.Count - 1)];
    }
}
