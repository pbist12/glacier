// File: EnemySpawner.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    // Dependencies
    private EnemyPoolHub hub;
    private BulletPoolHub bulletPool;
    private VerticalScrollerSimple vssample;

    #region Inspector

    [Header("Pool")]
    public bool usePooling = true;

    [Header("Normal Spawn (Ticker)")]
    [Tooltip("노말 스폰 on/off (외부에서 토글)")]
    public bool normalEnabled = false;

    [Min(0.01f)]
    public float interval = 1.0f;

    [Min(0)]
    public int spawnPerTick = 1;

    [Min(0)]
    [Tooltip("필드 전체 적 수 상한(성능 보호용)")]
    public int maxAlive = 50;

    [Tooltip("StageData가 없거나 비었을 때 균등 랜덤으로 사용 (폴백)")]
    private GameObject[] enemyPrefabs;

    [Header("Elite")]
    public bool eliteEnabled = false;
    private GameObject[] elitePrefabs;

    [Header("Boss")]
    public bool bossEnabled = false;
    public bool isBossSpawned = false;
    private GameObject[] bossPrefabs;
    public float bossSpawnDelay = 3f;

    [Header("Spawn Area (Rectangle)")]
    public Transform areaCenter;
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Anchors (optional)")]
    [Tooltip("CenterTop 지점을 직접 지정하고 싶다면 할당 (없으면 앵커 스캔/자동 계산)")]
    public Transform centerTopAnchor;

    [Header("Shop Portal")]
    public GameObject shopPortalPrefab;
    public GameObject spawnedShopPortal;

    [Header("Normal Group Order")]
    [Tooltip("true: 그룹을 라운드로빈(1마리씩) 순차 스폰, false: 한 그룹을 모두 소진하고 다음 그룹으로 이동")]
    public bool roundRobinGroups = true;

    public EnemyPoolHub Hub => hub;

    #endregion

    #region Runtime State

    private float _nextTickAt;
    private bool _eliteActive;

    // StageData 기반 "정확한 수량 + 앵커 스폰"을 위한 엔트리
    private readonly List<GroupEntry> _entries = new();
    private int _normalSpawnBudget; // 남은 노멀 스폰 총량(모든 그룹 spawnCount 합)

    private sealed class GroupEntry
    {
        public GameObject prefab;
        public int remaining; // 해당 그룹 남은 스폰 수
        public StageData.SpawnPoints[] spawnPoints; // 그룹 지정 포인트 타입들
        public bool cycle;     // 순환/랜덤
        public int spIdx;      // 순환 인덱스
    }

    // 그룹 순차 스폰 커서
    private int _groupCursor = 0;

    // ───────── 일반 전멸 이벤트 및 상태 ─────────
    public event System.Action OnNormalsCleared;
    private bool normalSpawningActive;   // 노멀 스폰 루프 진행 중?
    private bool normalsClearedSent;     // OnNormalsCleared 중복 방지

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        hub = FindFirstObjectByType<EnemyPoolHub>();
        vssample = FindFirstObjectByType<VerticalScrollerSimple>();
        bulletPool = FindFirstObjectByType<BulletPoolHub>();
    }

    private void OnEnable()
    {
        ResetTick();
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Start()
    {
        ResetTick();
    }

    private void Update()
    {
        TryTickNormal();
        CheckNormalsClearedGate(); // 전멸 게이트 체크(스폰 종료 후 0명 되는 순간)
    }

    private void OnValidate()
    {
        if (interval < 0.01f) interval = 0.01f;
        if (spawnPerTick < 0) spawnPerTick = 0;
        if (maxAlive < 0) maxAlive = 0;
        rectSize.x = Mathf.Max(0.01f, rectSize.x);
        rectSize.y = Mathf.Max(0.01f, rectSize.y);
    }

    private void OnDrawGizmosSelected()
    {
        var pivot = areaCenter ? areaCenter.position : transform.position;
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawWireCube(pivot, new Vector3(rectSize.x, rectSize.y, 0.1f));
    }

    #endregion

    #region Stage Data

    /// <summary>
    /// StageData를 읽어 노멀/엘리트/보스 프리팹과 "정확한 스폰 수량 + 앵커 포인트" 테이블을 구성한다.
    /// </summary>
    public void LoadStage(StageData stage)
    {
        _entries.Clear();
        _normalSpawnBudget = 0;
        _eliteActive = false;
        isBossSpawned = false;
        _groupCursor = 0;

        if (stage != null)
        {
            // Normal (정확한 수량 + 포인트 지정)
            if (stage.waves != null && stage.waves.groups != null)
            {
                foreach (var g in stage.waves.groups)
                {
                    if (g == null || g.monster == null || g.monster.prefab == null) continue;
                    int count = Mathf.Max(0, g.spawnCount);
                    if (count <= 0) continue;

                    _entries.Add(new GroupEntry
                    {
                        prefab = g.monster.prefab,
                        remaining = count,
                        spawnPoints = g.spawnPoints,
                        cycle = g.cycleSpawnPoints,
                        spIdx = 0
                    });
                    _normalSpawnBudget += count;
                }

                // 폴백용 균등 풀
                enemyPrefabs = stage.waves.groups
                    .Where(m => m != null && m.monster != null && m.monster.prefab != null)
                    .Select(m => m.monster.prefab)
                    .Distinct()
                    .ToArray();
            }

            // Elite
            if (stage.elites != null)
            {
                elitePrefabs = stage.elites
                    .Where(e => e != null && e.prefab != null)
                    .Select(e => e.prefab)
                    .Distinct()
                    .ToArray();
            }

            // Boss
            if (stage.bosses != null)
            {
                bossPrefabs = stage.bosses
                    .Where(b => b != null && b.bossPrefab != null)
                    .Select(b => b.bossPrefab)
                    .ToArray();
            }
        }

        ResetTick();
    }
    #endregion

    #region Phase Controls (External API)

    public void BeginNormalPhase()
    {
        // 스폰 예산이 있거나(정확 스폰) 폴백 프리팹이 있으면 ON
        normalEnabled = (_normalSpawnBudget > 0) || (enemyPrefabs != null && enemyPrefabs.Length > 0);
        ResetTick();

        normalSpawningActive = true;
        normalsClearedSent = false;
    }

    public void EnableNormal(bool on)
    {
        normalEnabled = on;
        if (!on)
        {
            // 노멀 스폰 중단 시점 → 전멸 게이트 준비
            normalSpawningActive = false;
            CheckNormalsClearedGate();
        }
    }

    public void BeginElitePhase()
    {
        eliteEnabled = elitePrefabs != null && elitePrefabs.Length > 0;
        if (eliteEnabled) SpawnElitePack();
    }
    public void EnableElite(bool on) => eliteEnabled = on;

    public void BeginBossPhaseWithDelay()
    {
        if (!bossEnabled) return;
        if (GameManager.Instance.State == GameManager.GameState.Boss)
        {
            Invoke(nameof(SpawnBoss), bossSpawnDelay);
        }
    }
    public void EnableBoss(bool on) => bossEnabled = on;

    public void ResetTick() => _nextTickAt = Time.time + interval;

    #endregion

    #region Normal Ticker

    private void TryTickNormal()
    {
        if (!normalEnabled) return;
        if (Time.time < _nextTickAt) return;
        if (spawnedShopPortal != null) return;
        if (GameManager.Instance.Paused) return;

        _nextTickAt = Time.time + interval;

        // 폭주 방지: 필드 전체 적 수 상한이면 스폰 보류
        if (CountAllAlive() >= maxAlive) return;

        // 예산을 소진하면서 스폰
        for (int i = 0; i < spawnPerTick; i++)
        {
            if (!TrySpawnNormalSequential())
                break; // 더 이상 스폰할 예산이 없으면 종료
        }

        // 예산을 다 썼으면 노멀 스폰을 멈춘다 → 이후 전멸 게이트가 엘리트로 넘김
        if (_normalSpawnBudget <= 0 && normalEnabled)
            EnableNormal(false);
    }

    /// <summary>
    /// 그룹 순차 스폰:
    /// - roundRobinGroups == true  → 1마리씩 라운드로빈(그룹들을 돌면서 한 마리씩)
    /// - roundRobinGroups == false → 한 그룹을 모두 소진하고 다음 그룹으로
    /// </summary>
    private bool TrySpawnNormalSequential()
    {
        if (_normalSpawnBudget <= 0)
        {
            // 폴백: StageData 없거나 모두 0인 경우 균등 랜덤 프리팹으로만 스폰 가능
            if (enemyPrefabs != null && enemyPrefabs.Length > 0)
            {
                var pf = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
                SpawnNormalOneAt(pf, GetRandomPointInRect());
                return true; // 폴백은 예산 개념이 없으므로 계속 가능
            }
            return false;
        }

        if (_entries.Count == 0) return false;

        if (roundRobinGroups)
        {
            // 라운드로빈: 커서부터 시작해서 남은 그룹을 찾아 1마리 스폰
            int tries = _entries.Count;
            while (tries-- > 0)
            {
                _groupCursor = ((_groupCursor % _entries.Count) + _entries.Count) % _entries.Count;
                var e = _entries[_groupCursor];

                if (e.remaining > 0)
                {
                    SpawnOneFromEntry(e);
                    // 다음번에는 다음 그룹부터 보게 커서 이동
                    _groupCursor = (_groupCursor + 1) % _entries.Count;
                    return true;
                }

                _groupCursor = (_groupCursor + 1) % _entries.Count;
            }

            // 남은 게 없다면 실패 처리
            return false;
        }
        else
        {
            // 그룹 소진 방식: 커서가 가리키는 그룹을 먼저 다 소진
            // 남은 게 없으면 다음 그룹으로 이동
            int tries = _entries.Count;
            while (tries-- > 0)
            {
                _groupCursor = ((_groupCursor % _entries.Count) + _entries.Count) % _entries.Count;
                var e = _entries[_groupCursor];

                if (e.remaining > 0)
                {
                    SpawnOneFromEntry(e);
                    // 소진 방식은 커서를 고정(해당 그룹을 먼저 다 쓰기)
                    if (e.remaining <= 0)
                        _groupCursor = (_groupCursor + 1) % _entries.Count;
                    return true;
                }

                _groupCursor = (_groupCursor + 1) % _entries.Count;
            }

            return false;
        }
    }

    private void SpawnOneFromEntry(GroupEntry e)
    {
        e.remaining = Mathf.Max(0, e.remaining - 1);
        _normalSpawnBudget = Mathf.Max(0, _normalSpawnBudget - 1);

        Vector3 pos = ResolveNormalSpawnPosition(e);
        SpawnNormalOneAt(e.prefab, pos);
    }

    private void SpawnNormalOneAt(GameObject prefab, Vector3 pos)
    {
        var go = Spawn(prefab, pos, Quaternion.identity);
        var eh = go.GetComponent<EnemyHealth>();
        if (eh) eh.Init(this, EnemyHealth.EnemyKind.Normal);
    }

    #endregion

    #region Elite & Boss

    public void SpawnElitePack()
    {
        if (!eliteEnabled || elitePrefabs == null || elitePrefabs.Length == 0) return;
        if (GameManager.Instance.State != GameManager.GameState.Elite) return;

        hub.DespawnAll();

        var pf = elitePrefabs[UnityEngine.Random.Range(0, elitePrefabs.Length)];
        var pos = ResolveCenterTopPosition();

        var go = Spawn(pf, pos, Quaternion.identity);
        var eh = go.GetComponent<EnemyHealth>();
        if (eh) eh.Init(this, EnemyHealth.EnemyKind.Elite);

        _eliteActive = true;
    }

    public void NotifyEliteUnitDead()
    {
        _eliteActive = false;
        if (!_eliteActive)
            GameManager.Instance?.OnEliteCleared();
    }

    private void SpawnBoss()
    {
        if (!bossEnabled || bossPrefabs == null || bossPrefabs.Length == 0) return;

        hub.DespawnAll();

        var pos = ResolveCenterTopPosition();
        var pf = bossPrefabs[UnityEngine.Random.Range(0, bossPrefabs.Length)];
        if (!isBossSpawned)
        {
            Spawn(pf, pos, Quaternion.identity);
            isBossSpawned = true;
        }
    }

    #endregion

    #region Shop Portal

    public void SpawnShopPortal()
    {
        GameManager.Instance.StartShopPhase();
        if (!shopPortalPrefab)
        {
            GameManager.Instance?.StartNormalPhase();
            return;
        }

        bulletPool.BombClearAll();
        hub.DespawnAll();
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;

        if (vssample) vssample.speed = vssample.minspeed;

        spawnedShopPortal = Spawn(shopPortalPrefab, pos, Quaternion.identity);
    }

    public void DeSpawnShopPortal()
    {
        if (spawnedShopPortal)
        {
            Destroy(spawnedShopPortal);
            spawnedShopPortal = null;

            if (vssample) vssample.speed = vssample.maxspeed;
        }
    }

    #endregion

    #region Spawn Helpers

    private GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        if (usePooling && hub != null)
            return hub.Spawn(prefab, pos, rot);

        return Instantiate(prefab, pos, rot);
    }

    private void Despawn(GameObject instance)
    {
        if (usePooling && hub != null)
            hub.Despawn(instance);
        else
            Destroy(instance);
    }

    private int CountAllAlive()
    {
        return EnemyHealth.ActiveCount;
    }

    private Vector3 GetRandomPointInRect()
    {
        var pivot = areaCenter ? areaCenter.position : transform.position;
        float x = UnityEngine.Random.Range(-rectSize.x * 0.5f, rectSize.x * 0.5f);
        float y = UnityEngine.Random.Range(-rectSize.y * 0.5f, rectSize.y * 0.5f);
        return pivot + new Vector3(x, y, 0f);
    }

    #endregion

    #region NEW: 위치 해석(일반/엘리트/보스) + 전멸 게이트

    // 일반 전멸 게이트 체크: 노멀 스폰이 멈췄고, 생존 0이면 1회 알림
    private void CheckNormalsClearedGate()
    {
        if (normalsClearedSent) return;
        if (normalEnabled) return;        // 아직 스폰 중이면 대기
        if (normalSpawningActive) return; // 스폰 종료 신호 전이면 대기
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Normal) return;

        if (CountAllAlive() == 0)
        {
            normalsClearedSent = true;
            OnNormalsCleared?.Invoke();
        }
    }

    // ───────── 일반 몬스터: 그룹의 앵커 규칙으로 좌표를 해석
    private Vector3 ResolveNormalSpawnPosition(GroupEntry e)
    {
        // 1) 그룹이 포인트 타입들을 지정했다면 그 규칙 사용
        if (e.spawnPoints != null && e.spawnPoints.Length > 0)
        {
            StageData.SpawnPoints type;
            if (e.cycle)
            {
                type = e.spawnPoints[(e.spIdx++) % e.spawnPoints.Length];
            }
            else
            {
                type = e.spawnPoints[UnityEngine.Random.Range(0, e.spawnPoints.Length)];
            }

            var t = ResolveSpawnPoint(type);
            if (t) return t.position;
        }

        // 2) 지정 포인트가 없거나 실패 → 전체 앵커에서 랜덤
        var any = GetComponentsInChildren<SpawnPointAnchor>(true)
                    ?.Where(a => a)
                    .Select(a => a.transform).ToList();
        if (any != null && any.Count > 0)
            return any[UnityEngine.Random.Range(0, any.Count)].position;

        // 3) 최종 폴백: 사각형 랜덤
        return GetRandomPointInRect();
    }

    // 포인트 타입 해석: CenterTop은 특별 처리, 그 외 타입은 해당 앵커 중에서 랜덤
    private Transform ResolveSpawnPoint(StageData.SpawnPoints type)
    {
        var anchors = GetComponentsInChildren<SpawnPointAnchor>(true);
        if (anchors == null || anchors.Length == 0)
        {
            // 앵커가 하나도 없는 씬이면 null (바깥에서 폴백 처리)
            return null;
        }

        if (type == StageData.SpawnPoints.CenterTop)
        {
            // CenterTop → Top-Center 해석(엘리트/보스와 동일 규칙)
            var centerTop = anchors.Where(a => a && a.pointType == StageData.SpawnPoints.CenterTop)
                                   .Select(a => a.transform).ToList();
            var ups = anchors.Where(a => a && a.pointType == StageData.SpawnPoints.Up)
                                   .Select(a => a.transform).ToList();
            var any = anchors.Where(a => a).Select(a => a.transform).ToList();

            var pick = PickTopCenter(centerTop);
            if (!pick && ups.Count > 0) pick = PickTopCenter(ups);
            if (!pick && any.Count > 0) pick = PickTopCenter(any);
            return pick; // 없으면 null
        }
        else
        {
            // 지정 타입에서 랜덤
            var list = anchors.Where(a => a && a.pointType == type).Select(a => a.transform).ToList();
            if (list.Count > 0)
                return list[UnityEngine.Random.Range(0, list.Count)];
            return null;
        }
    }

    // 엘리트/보스 전용: CenterTop → Up → 전체 → areaCenter 폴백
    private Vector3 ResolveCenterTopPosition()
    {
        // 1) 직접 지정된 앵커 최우선
        if (centerTopAnchor) return centerTopAnchor.position;

        // 2) SpawnPointAnchor 스캔 (자식 기준)
        var anchors = GetComponentsInChildren<SpawnPointAnchor>(true);
        if (anchors != null && anchors.Length > 0)
        {
            var centerTop = anchors.Where(a => a && a.pointType == StageData.SpawnPoints.CenterTop).Select(a => a.transform).ToList();
            var ups = anchors.Where(a => a && a.pointType == StageData.SpawnPoints.Up).Select(a => a.transform).ToList();
            var any = anchors.Where(a => a).Select(a => a.transform).ToList();

            var pick = PickTopCenter(centerTop);
            if (!pick && ups.Count > 0) pick = PickTopCenter(ups);
            if (!pick && any.Count > 0) pick = PickTopCenter(any);
            if (pick) return pick.position;
        }

        // 3) 폴백: 기존 중심
        return areaCenter ? areaCenter.position : transform.position;
    }

    private Transform PickTopCenter(List<Transform> list)
    {
        if (list == null || list.Count == 0) return null;
        var root = areaCenter ? areaCenter.position : transform.position;
        float maxY = float.NegativeInfinity;
        foreach (var t in list) if (t && t.position.y > maxY) maxY = t.position.y;

        Transform best = null; float bestDx = float.PositiveInfinity; const float eps = 0.0001f;
        foreach (var t in list)
        {
            if (!t) continue;
            if ((maxY - t.position.y) > eps) continue; // 최상단만
            float dx = Mathf.Abs(t.position.x - root.x);
            if (dx < bestDx) { bestDx = dx; best = t; }
        }
        return best;
    }

    #endregion
}
