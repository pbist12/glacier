using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Run")]
    public bool playOnStart = true;
    public float interval = 1.0f;     // �� �ʸ���
    public int spawnPerTick = 1;      // �� ���� �� ����
    public int maxAlive = 50;         // ���ÿ� �� ��������

    [Header("What to spawn")]
    public GameObject[] enemyPrefabs; // ���� ����(���� Ȯ��)
    public GameObject[] bossPrefabs;

    [Header("Where to spawn (Rectangle)")]
    public Transform areaCenter;      // ���� Spawner ��ġ ����
    public Vector2 rectSize = new Vector2(10, 6);

    [Header("Boss Spawn (Timer)")]
    public int bossScoreTrigger = 200;     // �� ������ �����ϸ� ���� ī��Ʈ�ٿ� ����
    public float bossSpawnDelay = 5f;      // ī��Ʈ�ٿ� ���� �ð�(��)
    public bool stopNormalSpawnsOnBoss = true; // ���� ī��Ʈ�ٿ� �����ϸ� �Ϲ� ���� �ߴ�

    float _next;

    // Boss Ÿ�̸� ����
    public bool _bossCountdownStarted = false;
    public bool _bossSpawned = false;
    public float _bossTimer = 0f;

    void Start()
    {
        if (playOnStart) _next = Time.time + interval;
    }

    void Update()
    {
        // --- ���� ���� ī��Ʈ�ٿ� Ʈ���� üũ ---
        TryArmBossCountdown();

        // --- �Ϲ� �� ���� ---
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

        // --- ���� ���� Ÿ�̸� ����/�Ϸ� ó�� ---
        HandleBossCountdown();
    }

    void TryArmBossCountdown()
    {
        if (_bossCountdownStarted || _bossSpawned) return;

        // ���� ���� ���� �� ī��Ʈ�ٿ� ����
        int score = GameManager.Instance ? GameManager.Instance.Score : 0;
        if (score >= bossScoreTrigger)
        {
            _bossCountdownStarted = true;
            _bossTimer = 0f;
            // �Ϲ� ������ ������ �ʴ� �����̶��, ���� �غ�Ǳ� ������ ���� ��� ����
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

        // areaCenter�� ������ �� ��ġ��, ������ ������ ��ġ�� ����
        Vector3 pos = areaCenter ? areaCenter.position : transform.position;
        Instantiate(prefab, pos, Quaternion.identity);

        // ������ �����ϸ� �Ϲ� ������ ���߰� �ʹٸ�, ���⼭ playOnStart�� ���� ��:
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
        // ���� �ܼ��� ���(�±� ���)
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
