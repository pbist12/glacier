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
    [Tooltip("�̸� ����ص� Ǯ ��� (��� ��Ÿ�ӿ� �ڵ� ������)")]
    public List<Entry> entries = new();

    // prefab Ű -> Ǯ
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    // �ν��Ͻ� -> ���� ������ ������
    private readonly Dictionary<GameObject, GameObject> _origin = new();

    void Awake()
    {
        // ���� ��ϵ� Ǯ ������
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

        // Ǯ ��ū(���� prefab ����) ����
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
            // entries���� parent�� maxSize ���� ������ ã�ƿͼ� parent�� ����(����ȭ)
            Transform parent = null;
            var entry = entries.Find(e => e.prefab == prefab);
            if (entry != null) parent = entry.parent;
            go = CreateInstance(prefab, parent);
        }

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        // �ɼ�: IPoolable ��
        if (go.TryGetComponent<IPoolable>(out var p)) p.OnSpawned();
        return go;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (!_origin.TryGetValue(instance, out var prefab))
        {
            // Ȥ�� ��ū�� ���ٸ�(�ܺ� ������) �ı�
            Destroy(instance);
            return;
        }

        // �ɼ�: IPoolable ��
        if (instance.TryGetComponent<IPoolable>(out var p)) p.OnDespawned();

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
        // ���� ����: Scene�� Ȱ��/�±׸� �ȴ� �� ��� ŭ �� �ʿ� �� ���� ���� ����
        if (string.IsNullOrEmpty(tag)) return 0;
        return GameObject.FindGameObjectsWithTag(tag).Length;
    }
}

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

// ���� ������ ������
public class PoolToken : MonoBehaviour
{
    public GameObject originPrefab;
}
