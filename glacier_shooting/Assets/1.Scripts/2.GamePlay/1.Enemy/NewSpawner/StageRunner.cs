// File: StageRunner.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class StageRunner : MonoBehaviour
{
    [Header("Data")]
    public StageData stage;

    [Header("Spawn Roots (Hierarchy에서 자동 수집)")]
    public Transform spawnRoot; // 비우면 this.transform
    private Dictionary<StageData.SpawnPoints, List<Transform>> _pointMap;

    [Header("Debug")]
    public bool autoRunOnStart = true;
    public bool logSpawn = false;

    // === 생존 추적 ===
    private readonly HashSet<GameObject> _aliveAll = new();     // 전체
    private readonly HashSet<GameObject> _aliveNormals = new(); // 일반 몬스터만

    private Coroutine _loopCo;

    private enum AliveType { Normal, Elite, Boss }

    void Awake()
    {
        BuildPointMap();
    }

    void Start()
    {
        if (autoRunOnStart && stage != null)
            _loopCo = StartCoroutine(RunStageLoop());
    }

    void OnDisable()
    {
        if (_loopCo != null) StopCoroutine(_loopCo);
        _loopCo = null;
        _aliveAll.Clear();
        _aliveNormals.Clear();
    }

    // ----- Spawn Point Map 구성 -----
    void BuildPointMap()
    {
        _pointMap = new();
        var root = spawnRoot != null ? spawnRoot : transform;
        foreach (var a in root.GetComponentsInChildren<SpawnPointAnchor>(true))
        {
            if (!_pointMap.TryGetValue(a.pointType, out var list))
            {
                list = new List<Transform>();
                _pointMap[a.pointType] = list;
            }
            list.Add(a.transform);
        }
        // 모든 Enum 키 존재 보장
        foreach (StageData.SpawnPoints t in System.Enum.GetValues(typeof(StageData.SpawnPoints)))
            if (!_pointMap.ContainsKey(t)) _pointMap[t] = new List<Transform>();
    }

    public void StopStage()
    {
        if (_loopCo != null) StopCoroutine(_loopCo);
        _loopCo = null;
    }

    // ----- 메인 루프 -----
    IEnumerator RunStageLoop()
    {
        do
        {
            // 1) 웨이브 한 바퀴(스폰 완료까지 대기)
            yield return RunWavesOnce();

            // 1-1) 일반 몬스터 전멸 대기
            yield return new WaitUntil(() => _aliveNormals.Count == 0);

            // 2) 엘리트 1마리 랜덤, 처치 대기 (CenterTop)
            if (stage.elites != null && stage.elites.Count > 0)
            {
                var elite = stage.elites[Random.Range(0, stage.elites.Count)];
                yield return SpawnOneAndWaitDeath(elite, AliveType.Elite, "Elite");
            }

            // 3) 보스 1마리 랜덤, 처치 대기 (CenterTop)
            if (stage.bosses != null && stage.bosses.Count > 0)
            {
                var boss = stage.bosses[Random.Range(0, stage.bosses.Count)];
                yield return SpawnOneAndWaitDeath(boss, AliveType.Boss, "Boss");
            }

            if (!stage.loop) break;

        } while (true);

        if (logSpawn) Debug.Log("[StageRunner] Stage finished.");
    }

    // ----- 웨이브 한 바퀴 -----
    IEnumerator RunWavesOnce()
    {
        if (stage.waves == null || stage.waves.groups == null || stage.waves.groups.Count == 0)
            yield break;

        var groups = stage.waves.groups;

        // “웨이브1 첫 스폰 시작”을 기준으로 각 그룹 startDelay 적용
        float baseStart = Time.time;
        bool firstStarted = false;
        int runningGroups = 0;

        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];

            if (!firstStarted)
            {
                runningGroups++;
                StartCoroutine(SpawnGroupRoutine(g,
                    captureFirstSpawnStart: () => { baseStart = Time.time; },
                    onComplete: () => { runningGroups--; }));
                firstStarted = true;
            }
            else
            {
                float wait = Mathf.Max(0f, (baseStart + g.startDelay) - Time.time);
                StartCoroutine(StartGroupDelayed(g, wait, () => runningGroups++, () => runningGroups--));
            }
        }

        // 모든 그룹 스폰이 끝날 때까지 대기 (적 전멸은 여기서 기다리지 않음)
        while (runningGroups > 0) yield return null;
    }

    IEnumerator StartGroupDelayed(SpawnGroup g, float delay, System.Action onStart, System.Action onDone)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        onStart?.Invoke();
        yield return SpawnGroupRoutine(g, captureFirstSpawnStart: null, onComplete: onDone);
    }

    // ----- 그룹 스폰(웨이브 몬스터) -----
    IEnumerator SpawnGroupRoutine(SpawnGroup g, System.Action captureFirstSpawnStart, System.Action onComplete)
    {
        if (g == null || g.monster == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        int count = (g.spawnCount <= 0) ? 1 : g.spawnCount;
        float interval = Mathf.Max(0f, g.interval);
        int spIdx = 0;

        for (int i = 0; i < count; i++)
        {
            if (i == 0) captureFirstSpawnStart?.Invoke();

            // ✅ 웨이브는 기존 spawnPoints 규칙(순환/랜덤) 그대로
            var posT = ChooseSpawnTransform(g, ref spIdx);
            var pos = (posT ? posT.position : transform.position);

            var go = InstantiateSafe(g.monster, pos, Quaternion.identity);
            if (logSpawn) Debug.Log($"[StageRunner] Spawn {g.monster.name} at {pos} (#{i + 1}/{count})");

            TrackAlive(go, AliveType.Normal);

            if (interval > 0f && i < count - 1)
                yield return new WaitForSeconds(interval);
        }

        onComplete?.Invoke();
    }

    // ----- 스폰 포인트 해석 -----
    // CenterTop 같이 “의미 기반” 타입을 자동 해석하도록 보강
    Transform ChooseSpawnTransform(SpawnGroup g, ref int spIdx)
    {
        var arr = g.spawnPoints;
        Transform chosen = null;

        if (arr != null && arr.Length > 0)
        {
            if (g.cycleSpawnPoints)
                chosen = ResolveSpawnPoint(arr[(spIdx++) % arr.Length]);
            else
                chosen = ResolveSpawnPoint(arr[Random.Range(0, arr.Length)]);
        }

        if (chosen == null)
        {
            // 지정 실패 시 모든 앵커 중 랜덤
            var all = _pointMap.SelectMany(kv => kv.Value).ToList();
            if (all.Count > 0) chosen = all[Random.Range(0, all.Count)];
        }
        return chosen != null ? chosen : transform;
    }

    Transform ResolveSpawnPoint(StageData.SpawnPoints type)
    {
        if (type == StageData.SpawnPoints.CenterTop)
            return GetCenterTopTransform();

        if (_pointMap.TryGetValue(type, out var list) && list != null && list.Count > 0)
            return list[Random.Range(0, list.Count)];

        return null;
    }

    // CenterTop 선택 규칙:
    // 1) CenterTop 앵커가 있으면 그중 최상단(Y 최대) & 중앙(X가 root.x에 가장 가까운) 우선
    // 2) 없으면 Up 앵커에서 동일 규칙
    // 3) 그래도 없으면 전체 앵커에서 동일 규칙
    // 4) 전혀 없으면 root
    Transform GetCenterTopTransform()
    {
        var root = spawnRoot != null ? spawnRoot : transform;
        Vector3 center = root.position;

        // 1) CenterTop 앵커
        if (_pointMap.TryGetValue(StageData.SpawnPoints.CenterTop, out var centers) &&
            centers != null && centers.Count > 0)
        {
            return PickTopCenter(centers, center) ?? centers[0];
        }

        // 2) Up 앵커
        if (_pointMap.TryGetValue(StageData.SpawnPoints.Up, out var ups) &&
            ups != null && ups.Count > 0)
        {
            var pick = PickTopCenter(ups, center);
            if (pick != null) return pick;
        }

        // 3) 전체 앵커
        var all = _pointMap.SelectMany(kv => kv.Value).ToList();
        if (all.Count > 0)
        {
            var pick = PickTopCenter(all, center);
            if (pick != null) return pick;
        }

        // 4) 폴백
        return root;
    }

    Transform PickTopCenter(List<Transform> candidates, Vector3 center)
    {
        if (candidates == null || candidates.Count == 0) return null;

        float maxY = float.NegativeInfinity;
        for (int i = 0; i < candidates.Count; i++)
        {
            var t = candidates[i];
            if (!t) continue;
            if (t.position.y > maxY) maxY = t.position.y;
        }

        const float eps = 0.0001f;
        Transform best = null;
        float bestDx = float.PositiveInfinity;

        for (int i = 0; i < candidates.Count; i++)
        {
            var t = candidates[i];
            if (!t) continue;
            if ((maxY - t.position.y) > eps) continue; // 최상단만

            float dx = Mathf.Abs(t.position.x - center.x);
            if (dx < bestDx)
            {
                bestDx = dx;
                best = t;
            }
        }
        return best;
    }

    // ----- 엘리트/보스 1체 스폰 & 처치 대기 -----
    IEnumerator SpawnOneAndWaitDeath(Object asset, AliveType type, string label)
    {
        // ✅ 엘리트/보스는 CenterTop 강제
        var t = GetCenterTopTransform();
        Vector3 pos = (t ? t.position : transform.position);

        var go = InstantiateSafe(asset, pos, Quaternion.identity);
        if (logSpawn) Debug.Log($"[StageRunner] Spawn {label}: {asset.name} at {pos}");

        TrackAlive(go, type);

        // 해당 1개 죽을 때까지 대기
        while (go != null) yield return null;
    }

    // ----- 인스턴스 생성/추적 -----
    GameObject InstantiateSafe(Object asset, Vector3 pos, Quaternion rot)
    {
        var prefab = ExtractPrefab(asset);
        if (prefab == null)
        {
            Debug.LogWarning($"[StageRunner] {asset.name}에서 프리팹을 찾지 못해 더미로 대체됩니다.");
            var dummy = new GameObject($"[Spawn:{asset.name}]");
            dummy.transform.SetPositionAndRotation(pos, rot);
            return dummy;
        }
        return Instantiate(prefab, pos, rot);
    }

    GameObject ExtractPrefab(Object asset)
    {
        if (asset is GameObject go) return go;

        var t = asset.GetType();
        var candidates = new[] { "prefab", "enemyPrefab", "bossPrefab", "elitePrefab", "mobPrefab" };
        foreach (var name in candidates)
        {
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && typeof(GameObject).IsAssignableFrom(f.FieldType))
                return (GameObject)f.GetValue(asset);
        }
        return null;
    }

    void TrackAlive(GameObject obj, AliveType type)
    {
        if (obj == null) return;

        _aliveAll.Add(obj);
        if (type == AliveType.Normal) _aliveNormals.Add(obj);

        var relay = obj.GetComponent<DeathRelay>();
        if (relay == null) relay = obj.AddComponent<DeathRelay>();
        relay.OnDied += () =>
        {
            _aliveAll.Remove(obj);
            _aliveNormals.Remove(obj);
        };
    }
}
