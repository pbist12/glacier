// File: EnemySpawner.cs (수정본)
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool")]
    [Tooltip("적/보스/포털을 풀링하고 싶다면 할당")]
    public EnemyPoolHub hub;
    public bool usePooling = true;

    [Header("Normal Spawn")]
    public bool normalEnabled = false;
    public float interval = 1.0f;
    public int spawnPerTick = 1;
    public int maxAlive = 50;
    public GameObject[] enemyPrefabs;

    [Header("Elite")]
    public bool eliteEnabled = false;
    public int eliteCount = 1;
    public GameObject[] elitePrefabs;

    [Header("Boss")]
    public bool bossEnabled = false;
    public GameObject[] bossPrefabs;
    public GameObject finalBossPrefab;
    public float bossSpawnDelay = 3f; // 스토리 연출 시간

    [Header("Area (Rectangle)")]
    public Transform areaCenter;
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Shop Portal")]
    public GameObject shopPortalPrefab;
    public GameObject spawnedShopPortal;

    float _next;
    int _aliveNormal = 0;
    int _aliveElite = 0;

    [Header("Refs")]
    public VerticalScrollerSimple vssample;

    void Start()
    {
        _next = Time.time + interval;
        vssample = FindFirstObjectByType<VerticalScrollerSimple>();
    }

    void Update()
    {
        if (normalEnabled && Time.time >= _next)
        {
            _next = Time.time + interval;
            // 성능 최적: 태그 스캔 대신 내부 카운터 사용 권장(아래 주석 참고)
            if (CountAllAlive() < maxAlive)
                for (int i = 0; i < spawnPerTick; i++) SpawnNormalOne();
        }
    }

    // ===== API (GameManager가 호출) =====
    public void EnableNormal(bool on) { normalEnabled = on; }
    public void EnableElite(bool on) { eliteEnabled = on; }
    public void EnableBoss(bool on) { bossEnabled = on; }
    public void ResetTick()
    {
        _next = Time.time + interval;
    }

    public void SpawnElitePack()
    {
        if (!eliteEnabled || elitePrefabs == null || elitePrefabs.Length == 0) return;

        _aliveElite = 0;
        for (int i = 0; i < Mathf.Max(1, eliteCount); i++)
        {
            var pf = elitePrefabs[Random.Range(0, elitePrefabs.Length)];
            var pos = areaCenter.transform.position;
            var go = Spawn(pf, pos, Quaternion.identity);
            var eh = go.GetComponent<EnemyHealth>();
            if (eh) eh.Init(this, EnemyHealth.EnemyKind.Elite);
            _aliveElite++;
        }
    }

    public void NotifyEliteUnitDead()
    {
        _aliveElite = Mathf.Max(0, _aliveElite - 1);
        if (_aliveElite == 0)
        {
            GameManager.Instance.OnEliteCleared();
        }
    }

    public void ArmBossCountdownOrSpawn()
    {
        if (!bossEnabled) return;
        Invoke(nameof(SpawnBoss), bossSpawnDelay);
    }

    public void SpawnFinalBoss()
    {
        if (!bossEnabled || !finalBossPrefab) return;
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Spawn(finalBossPrefab, pos, Quaternion.identity);
    }

    public void SpawnShopPortal()
    {
        if (!shopPortalPrefab) { GameManager.Instance.StartNormalPhase(); return; }
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        vssample.speed = vssample.minspeed;
        spawnedShopPortal = Spawn(shopPortalPrefab, pos, Quaternion.identity);
    }

    public void DeSpawnShopPortal()
    {
        if (!shopPortalPrefab) { GameManager.Instance.StartNormalPhase(); return; }
        if (spawnedShopPortal)
        {
            Despawn(spawnedShopPortal);
            vssample.speed = vssample.maxspeed;
            spawnedShopPortal = null;
        }
    }

    // ===== 내부 =====
    void SpawnNormalOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        var pf = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        var pos = GetRandomPointInRect();
        var go = Spawn(pf, pos, Quaternion.identity);
        var eh = go.GetComponent<EnemyHealth>();
        if (eh) eh.Init(this, EnemyHealth.EnemyKind.Normal);
        _aliveNormal++;
    }

    void SpawnBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0) return;
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Spawn(bossPrefabs[Random.Range(0, bossPrefabs.Length)], pos, Quaternion.identity);
    }

    // 풀/일반 공용 스폰 헬퍼
    GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (usePooling && hub != null)
            return hub.Spawn(prefab, pos, rot);
        else
            return Instantiate(prefab, pos, rot);
    }

    void Despawn(GameObject instance)
    {
        if (instance == null) return;
        if (usePooling && hub != null)
            hub.Despawn(instance);
        else
            Destroy(instance);
    }

    int CountAllAlive()
    {
        // 권장) 내부 카운터로 대체:
        // return _aliveNormal + _aliveElite; // + 보스 카운트
        // 간단 호환 유지용(성능 낮음):
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    Vector3 GetRandomPointInRect()
    {
        var pivot = areaCenter ? areaCenter.position : transform.position;
        float x = Random.Range(-rectSize.x * 0.5f, rectSize.x * 0.5f);
        float y = Random.Range(-rectSize.y * 0.5f, rectSize.y * 0.5f);
        return pivot + new Vector3(x, y, 0f);
    }

    void OnDrawGizmosSelected()
    {
        var pivot = areaCenter ? areaCenter.position : transform.position;
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawWireCube(pivot, new Vector3(rectSize.x, rectSize.y, 0.1f));
    }

    // 적이 죽거나 사라질 때 스포너가 호출하도록 만들면 카운트 정확해짐
    public void NotifyEnemyDespawned(EnemyHealth.EnemyKind kind)
    {
        if (kind == EnemyHealth.EnemyKind.Normal) _aliveNormal = Mathf.Max(0, _aliveNormal - 1);
        else if (kind == EnemyHealth.EnemyKind.Elite) _aliveElite = Mathf.Max(0, _aliveElite - 1);
    }
}
