using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public WaveSet waveSet;
    public Transform defaultSpawnPoint; // spawnPoints�� ����� �� ���

    float startTime;
    int nextWaveIndex;  // 0-based (Y=1�� index 0)
    bool running;

    void Start()
    {
        if (waveSet == null || waveSet.waves.Count == 0)
        {
            Debug.LogWarning("WaveSet�� ����ֽ��ϴ�.");
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

        // �̹��� �����ؾ� �� ���̺갡 �ִٸ� ��� ó�� (������ ��ŵ ��� while)
        while (ShouldStartNextWave(out float scheduledAt))
        {
            var wave = waveSet.waves[nextWaveIndex];
            StartCoroutine(SpawnWave(wave));    // ���� ���� �����־ ����
            nextWaveIndex++;

            // ���� �ɼ�
            if (nextWaveIndex >= waveSet.waves.Count)
            {
                if (waveSet.loop)
                {
                    nextWaveIndex = 0;
                    // ���������� ���� ������ �����Ϸ��� startTime�� �缳��
                    startTime = Time.time; // ���� ȸ������ 0���� ��⵿
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
        // ���̺� ���� �׷���� ���ķ� ���� (�� �׷��� �ڽŸ��� startDelay/interval�� ����)
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

            //TODO. ������Ʈ Ǯ?
            Instantiate(g.enemyPrefab, pos, rot);

            if (g.interval > 0f) yield return new WaitForSeconds(g.interval);
        }
    }
}
