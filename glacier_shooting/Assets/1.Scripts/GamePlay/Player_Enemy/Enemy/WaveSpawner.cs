using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public WaveSet waveSet;
    public Transform defaultSpawnPoint; // spawnPoints가 비었을 때 사용

    float startTime;
    int nextWaveIndex;  // 0-based (Y=1은 index 0)
    bool running;

    void Start()
    {
        if (waveSet == null || waveSet.waves.Count == 0)
        {
            Debug.LogWarning("WaveSet이 비어있습니다.");
            enabled = false;
            return;
        }
        startTime = Time.time;
        nextWaveIndex = 0;
        running = true;
    }

    void Update()
    {
        if (!running) return;

        // 이번에 시작해야 할 웨이브가 있다면 모두 처리 (프레임 스킵 대비 while)
        while (ShouldStartNextWave(out float scheduledAt))
        {
            var wave = waveSet.waves[nextWaveIndex];
            StartCoroutine(SpawnWave(wave));    // 이전 적이 남아있어도 시작
            nextWaveIndex++;

            // 루프 옵션
            if (nextWaveIndex >= waveSet.waves.Count)
            {
                if (waveSet.loop)
                {
                    nextWaveIndex = 0;
                    // 루프에서도 절대 스케줄 유지하려면 startTime을 재설정
                    startTime = Time.time; // 루프 회차마다 0으로 재기동
                }
                else
                {
                    running = false;
                    break;
                }
            }
        }
    }

    bool ShouldStartNextWave(out float scheduledAt)
    {
        scheduledAt = startTime + (nextWaveIndex) * waveSet.periodN; // (Y-1) * N
        return Time.time >= scheduledAt && running;
    }

    IEnumerator SpawnWave(WaveDef wave)
    {
        // 웨이브 내부 그룹들을 병렬로 시작 (각 그룹은 자신만의 startDelay/interval을 가짐)
        foreach (var g in wave.groups)
            StartCoroutine(SpawnGroupRoutine(g));

        yield break;
    }

    IEnumerator SpawnGroupRoutine(SpawnGroup g)
    {
        if (g == null || g.enemyPrefab == null) yield break;
        if (g.startDelay > 0f) yield return new WaitForSeconds(g.startDelay);

        int spawnLen = (g.spawnPoints != null && g.spawnPoints.Length > 0) ? g.spawnPoints.Length : 0;
        int spawnIdx = 0;

        for (int i = 0; i < g.count; i++)
        {
            Transform spawnPoint = defaultSpawnPoint;
            if (spawnLen > 0)
            {
                if (g.cycleSpawnPoints)
                {
                    spawnPoint = g.spawnPoints[spawnIdx % spawnLen];
                    spawnIdx++;
                }
                else
                {
                    spawnPoint = g.spawnPoints[Random.Range(0, spawnLen)];
                }
            }

            Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            //TODO. 오브젝트 풀?
            Instantiate(g.enemyPrefab, pos, rot);

            if (g.interval > 0f) yield return new WaitForSeconds(g.interval);
        }
    }
}
