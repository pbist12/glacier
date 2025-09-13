using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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

    float _next;
    int _aliveNormal = 0;
    int _aliveElite = 0;

    void Start()
    {
        _next = Time.time + interval;
    }

    void Update()
    {
        if (normalEnabled && Time.time >= _next)
        {
            _next = Time.time + interval;
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
        _next = Time.time + interval; // 과거에 멈춰 있던 타이머를 현재 기준으로 초기화
    }
    public void SpawnElitePack()
    {
        if (!eliteEnabled || elitePrefabs == null || elitePrefabs.Length == 0) return;

        _aliveElite = 0;
        for (int i = 0; i < Mathf.Max(1, eliteCount); i++)
        {
            var pf = elitePrefabs[Random.Range(0, elitePrefabs.Length)];
            var pos = GetRandomPointInRect();
            var go = Instantiate(pf, pos, Quaternion.identity);
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
        Invoke(nameof(SpawnBoss), bossSpawnDelay); // 간단히 지연 스폰(연출)
    }

    public void SpawnFinalBoss()
    {
        if (!bossEnabled || !finalBossPrefab) return;
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Instantiate(finalBossPrefab, pos, Quaternion.identity);
    }

    public void SpawnShopPortal()
    {
        if (!shopPortalPrefab) { GameManager.Instance.StartNormalPhase(); return; }
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Instantiate(shopPortalPrefab, pos, Quaternion.identity);
    }

    // ===== 내부 =====
    void SpawnNormalOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        var pf = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        var pos = GetRandomPointInRect();
        var go = Instantiate(pf, pos, Quaternion.identity);
        var eh = go.GetComponent<EnemyHealth>();
        if (eh) eh.Init(this, EnemyHealth.EnemyKind.Normal);
        _aliveNormal++;
    }

    void SpawnBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0) return;
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Instantiate(bossPrefabs[Random.Range(0, bossPrefabs.Length)], pos, Quaternion.identity);
    }

    int CountAllAlive()
    {
        // 간단히 태그 기반(필요시 레지스트리로 개선)
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
}
