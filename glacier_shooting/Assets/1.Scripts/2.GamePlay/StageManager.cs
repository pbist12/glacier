using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stages (순서대로 진행)")]
    public StageData[] stages;

    [SerializeField] public int _stageIndex;       // 현재 스테이지 인덱스
    [SerializeField] private BossAsset _stageBoss; // 이번 스테이지에 선택된 보스
    public bool isEnd;

    public int CurrentStageIndex => _stageIndex;
    public bool IsCampaignDone => stages == null || _stageIndex >= (stages?.Length ?? 0);
    public bool BossSelected => _stageBoss != null;
    public BossAsset SelectedBoss => _stageBoss;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

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

    // 스포너가 필요하면 가져다 쓰는 헬퍼들 (옵션)
    public SpawnGroup[] GetCurrentStageMobs()
    {
        var s = stages[_stageIndex];
        if (s == null || s.waves.groups == null) return System.Array.Empty<SpawnGroup>();
        return s.waves.groups.ToArray();
    }

    public GameObject[] GetCurrentStageElites()
    {
        var s = stages[_stageIndex];
        if (s == null || s.elites == null || s.elites.Count == 0) return System.Array.Empty<GameObject>();
        var arr = new GameObject[s.elites.Count];
        for (int i = 0; i < s.elites.Count; i++) arr[i] = s.elites[i]?.prefab;
        return arr;
    }

    public GameObject GetSelectedBossPrefab()
    {
        if (_stageBoss == null) return null;

        var t = typeof(BossAsset);
        var fi = t.GetField("bossPrefab") ?? t.GetField("prefab");
        return fi != null ? (GameObject)fi.GetValue(_stageBoss) : null;
    }

    private void SelectRandomBossForCurrentStage()
    {
        _stageBoss = null;
        var s = stages[_stageIndex];
        if (s == null || s.bosses == null || s.bosses.Count == 0) return;

        // 상한 exclusive로 마지막 보스가 제외되지 않도록
        _stageBoss = s.bosses[Random.Range(0, s.bosses.Count)];
    }
}
