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

    [Tooltip("StageData가 없거나 비었을 때 균등 랜덤으로 사용")]
    public GameObject[] enemyPrefabs;

    [Header("Elite")]
    public bool eliteEnabled = false;
    public GameObject[] elitePrefabs;

    [Header("Boss")]
    public bool bossEnabled = false;
    public bool isBossSpawned = false;
    public GameObject[] bossPrefabs;
    public float bossSpawnDelay = 3f;

    [Header("Spawn Area (Rectangle)")]
    public Transform areaCenter;
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Shop Portal")]
    public GameObject shopPortalPrefab;
    public GameObject spawnedShopPortal; // 읽기 전용 표시용(원하면 커스텀 어트리뷰트)

    public EnemyPoolHub Hub => hub;

    #endregion

    #region Runtime State

    private float _nextTickAt;
    private bool _eliteActive;

    // StageData 기반 노말 가중치 테이블
    private readonly List<WeightedEntry> _weighted = new();

    private sealed class WeightedEntry
    {
        public GameObject prefab;
        public int weight;
    }

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
    /// StageData를 읽어 노말/엘리트/보스 프리팹과 가중치 테이블을 구성한다.
    /// spawnCount는 '가중치'로만 쓰고 감소/카운팅은 하지 않는다.
    /// </summary>
    public void LoadStage(StageData stage)
    {
        _weighted.Clear();
        _eliteActive = false;
        isBossSpawned = false;

        if (stage != null)
        {
            // Normal (weights)
            if (stage.waves != null)
            {
                // 균등 풀(폴백용)
                enemyPrefabs = stage.waves.groups
                    .Where(m => m != null && m.monster.prefab != null)
                    .Select(m => m.monster.prefab)
                    .Distinct()
                    .ToArray();

                foreach (var m in stage.waves.groups)
                {
                    if (m == null || m.monster.prefab == null) continue;
                    int weight = Mathf.Max(1, m.monster.spawnCount); // 최소 1
                    _weighted.Add(new WeightedEntry { prefab = m.monster.prefab, weight = weight });
                }
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
        // 가중치/프리팹 중 하나라도 있으면 ON
        normalEnabled = (_weighted.Count > 0) || (enemyPrefabs != null && enemyPrefabs.Length > 0);
        ResetTick();
    }
    public void EnableNormal(bool on) => normalEnabled = on;
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

        // 폭주 방지: 필드 전체 적 수가 상한이면 스폰 보류
        if (CountAllAlive() >= maxAlive) return;

        for (int i = 0; i < spawnPerTick; i++)
            TrySpawnNormalWeighted();
    }

    private void TrySpawnNormalWeighted()
    {
        // StageData 기반 가중치가 있으면 우선 사용
        if (_weighted.Count > 0)
        {
            var pick = PickWeightedPrefab(_weighted);
            if (pick != null)
            {
                SpawnNormalOne(pick);
                return;
            }
        }

        // 폴백: 균등 랜덤
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            var pf = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            SpawnNormalOne(pf);
        }
    }

    private static GameObject PickWeightedPrefab(List<WeightedEntry> list)
    {
        int total = 0;
        for (int i = 0; i < list.Count; i++)
            total += Mathf.Max(0, list[i].weight);

        if (total <= 0) return null;

        int r = UnityEngine.Random.Range(0, total); // [0, total)
        int acc = 0;
        for (int i = 0; i < list.Count; i++)
        {
            acc += Mathf.Max(0, list[i].weight);
            if (r < acc) return list[i].prefab;
        }
        return null;
    }

    private void SpawnNormalOne(GameObject prefab)
    {
        var pos = GetRandomPointInRect();
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

        var pf = elitePrefabs[UnityEngine.Random.Range(0, elitePrefabs.Length)]; // 상한 exclusive
        var pos = areaCenter ? areaCenter.position : transform.position;

        var go = Spawn(pf, pos, Quaternion.identity);
        var eh = go.GetComponent<EnemyHealth>();
        if (eh) eh.Init(this, EnemyHealth.EnemyKind.Elite);

        _eliteActive = true;
    }

    public void NotifyEliteUnitDead()
    {
        // 여러 마리 관리가 필요하면 별도 집계로 확장
        _eliteActive = false;
        if (!_eliteActive)
        {
            GameManager.Instance?.OnEliteCleared();
        }
    }

    private void SpawnBoss()
    {
        if (!bossEnabled || bossPrefabs == null || bossPrefabs.Length == 0) return;

        hub.DespawnAll();

        var pos = areaCenter ? areaCenter.position : transform.position;
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
}
