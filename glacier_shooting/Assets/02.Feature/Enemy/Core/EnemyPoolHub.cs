// File: EnemyPoolHub.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyPoolHub : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public GameObject prefab;
        [Min(0)] public int prewarm = 0;
        [Min(1)] public int maxSize = 256;
        public Transform parent;
    }

    [Header("Pools")]
    [Tooltip("미리 등록해둘 풀 목록 (없어도 런타임에 자동 생성됨)")]
    public List<Entry> entries = new();

    // prefab 키 -> 풀
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    // 인스턴스 -> 원본 프리팹 역추적
    private readonly Dictionary<GameObject, GameObject> _origin = new();

    private readonly HashSet<GameObject> _active = new();
    public int ActiveCount => _active.Count;

    void Awake()
    {
        // 사전 등록된 풀 프리웜
        foreach (var e in entries)
        {
            if (e.prefab == null) continue;
            var q = GetOrCreatePool(e.prefab);
            for (int i = 0; i < e.prewarm; i++)
            {
                var go = CreateInstance(e.prefab, transform);
                InternalRelease(e.prefab, go);
            }
        }
    }

    Queue<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>(32);
            _pools[prefab] = q;
        }
        return q;
    }

    GameObject CreateInstance(GameObject prefab, Transform parent = null)
    {
        var go = Instantiate(prefab, parent);
        go.SetActive(false);

        // 풀 토큰(원본 prefab 참조) 부착
        var token = go.GetComponent<PoolToken>();
        if (token == null) token = go.AddComponent<PoolToken>();
        token.originPrefab = prefab;

        _origin[go] = prefab;
        return go;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        var pool = GetOrCreatePool(prefab);
        GameObject go = null;

        if (pool.Count > 0)
        {
            go = pool.Dequeue();
        }
        else
        {
            Transform parent = null;
            var entry = entries.Find(e => e.prefab == prefab);
            if (entry != null) parent = entry.parent;
            go = CreateInstance(prefab, parent);
        }

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        if (go.TryGetComponent<IPoolable>(out var p)) p.OnSpawned();

        // ✅ 활성 목록에 등록
        _active.Add(go);
        return go;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (!_origin.TryGetValue(instance, out var prefab))
        {
            Destroy(instance);
            return;
        }

        if (instance.TryGetComponent<IPoolable>(out var p)) p.OnDespawned();

        // ✅ 활성 목록에서 제거
        _active.Remove(instance);

        InternalRelease(prefab, instance);
    }

    void InternalRelease(GameObject prefab, GameObject instance)
    {
        instance.SetActive(false);
        var pool = GetOrCreatePool(prefab);
        pool.Enqueue(instance);
    }

    public int ActiveCountRough(string tag = null)
    {
        // 간단 추정: Scene에 활성/태그를 훑는 건 비용 큼 → 필요 시 직접 관리 권장
        if (string.IsNullOrEmpty(tag)) return 0;
        return GameObject.FindGameObjectsWithTag(tag).Length;
    }
    public int DespawnAll()
    {
        var snapshot = new List<GameObject>(_active);
        int count = 0;
        foreach (var go in snapshot)
        {
            if (go != null && go.activeInHierarchy)
            {
                Despawn(go);
                count++;
            }
        }
        return count;
    }
}

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

public class PoolToken : MonoBehaviour
{
    public GameObject originPrefab;
}
