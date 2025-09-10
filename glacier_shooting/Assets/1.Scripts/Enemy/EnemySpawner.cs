using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Run")]
    public bool playOnStart = true;
    public float interval = 1.0f;     // 몇 초마다
    public int spawnPerTick = 1;      // 한 번에 몇 마리
    public int maxAlive = 50;         // 동시에 몇 마리까지

    [Header("What to spawn")]
    public GameObject[] enemyPrefabs; // 랜덤 선택(동일 확률)
    public GameObject[] bossPrefabs;

    [Header("Where to spawn (Rectangle)")]
    public Transform areaCenter;      // 비우면 Spawner 위치 기준
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Boss Spawn (Timer)")]
    public int bossScoreTrigger = 200;     // 이 점수에 도달하면 보스 카운트다운 시작
    public float bossSpawnDelay = 5f;      // 카운트다운 지속 시간(초)
    public bool stopNormalSpawnsOnBoss = true; // 보스 카운트다운 시작하면 일반 스폰 중단

    float _next;

    // Boss 타이머 상태
    public bool _bossCountdownStarted = false;
    public bool _bossSpawned = false;
    public float _bossTimer = 0f;

    void Start()
    {
        if (playOnStart) _next = Time.time + interval;
    }

    void Update()
    {
        // --- 보스 스폰 카운트다운 트리거 체크 ---
        TryArmBossCountdown();

        // --- 일반 적 스폰 ---
        if (playOnStart && !(_bossCountdownStarted && stopNormalSpawnsOnBoss))
        {
            if (Time.time >= _next)
            {
                _next += interval;

                if (CountAlive() < maxAlive)
                {
                    for (int i = 0; i < spawnPerTick; i++)
                        SpawnOne();
                }
            }
        }

        // --- 보스 스폰 타이머 진행/완료 처리 ---
        HandleBossCountdown();
    }

    void TryArmBossCountdown()
    {
        if (_bossCountdownStarted || _bossSpawned) return;

        // 점수 조건 충족 시 카운트다운 시작
        int score = GameManager.Instance ? GameManager.Instance.Score : 0;
        if (score >= bossScoreTrigger)
        {
            _bossCountdownStarted = true;
            _bossTimer = 0f;
            // 일반 스폰을 멈추지 않는 설정이라면, 보스 준비되기 전에도 적은 계속 나옴
        }
    }

    void HandleBossCountdown()
    {
        if (!_bossCountdownStarted || _bossSpawned) return;

        _bossTimer += Time.deltaTime;
        if (_bossTimer >= bossSpawnDelay)
        {
            SpawnBoss();
            _bossSpawned = true;
            _bossCountdownStarted = false;
        }
    }

    void SpawnOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        var pos = GetRandomPointInRect();
        Instantiate(prefab, pos, Quaternion.identity);
    }

    void SpawnBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0) return;

        var prefab = bossPrefabs[Random.Range(0, bossPrefabs.Length)];

        // areaCenter가 있으면 그 위치에, 없으면 스포너 위치에 스폰
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Instantiate(prefab, pos, Quaternion.identity);

        // 보스가 등장하면 일반 스폰을 멈추고 싶다면, 여기서 playOnStart를 꺼도 됨:
        // playOnStart = false;
    }

    Vector3 GetRandomPointInRect()
    {
        var pivot = areaCenter ? areaCenter.position : transform.position;
        float x = Random.Range(-rectSize.x * 0.5f, rectSize.x * 0.5f);
        float y = Random.Range(-rectSize.y * 0.5f, rectSize.y * 0.5f);
        return pivot + new Vector3(x, y, 0f);
    }

    int CountAlive()
    {
        // 가장 단순한 방식(태그 기반)
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    void OnDrawGizmosSelected()
    {
        var pivot = areaCenter ? areaCenter.position : transform.position;
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawWireCube(pivot, new Vector3(rectSize.x, rectSize.y, 0.1f));
    }
}
