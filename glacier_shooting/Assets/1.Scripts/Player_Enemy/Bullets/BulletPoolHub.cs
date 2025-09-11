// File: BulletPoolHub.cs
using System.Collections.Generic;
using UnityEngine;

public enum BulletPoolKey { Player, Enemy, Enemy_Homing }

[DisallowMultipleComponent]
public class BulletPoolHub : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public BulletPoolKey key;
        public Bullet bulletPrefab;
        [Min(0)] public int initialCount = 256;
        public bool expandable = true;
        [Min(1)] public int expandBlock = 64;

        [Header("Optional")]
        public string sortingLayerName; // ���� ������(2D���)
        public int orderInLayer;
        public string tagOverride;      // "PlayerBullet" / "EnemyBullet" ��
        public int layerOverride = -1;  // 0 �̻��̸� ����
    }

    [Header("Pools")]
    public List<Entry> entries = new();

    // ���� ����
    readonly Dictionary<BulletPoolKey, Stack<Bullet>> _stacks = new();
    readonly Dictionary<BulletPoolKey, Entry> _cfg = new();

    // Ȱ�� ź ����(��ź/���� ��ɿ�)
    readonly Dictionary<BulletPoolKey, HashSet<Bullet>> _activeByKey = new();
    readonly HashSet<Bullet> _activeAll = new();

    Transform _root;

    void Awake()
    {
        _root = transform;
        foreach (var e in entries)
        {
            _cfg[e.key] = e;
            _stacks[e.key] = new Stack<Bullet>(Mathf.Max(16, e.initialCount));
            _activeByKey[e.key] = new HashSet<Bullet>();
            Prewarm(e, _stacks[e.key], e.initialCount);
        }
    }

    void Prewarm(Entry e, Stack<Bullet> stack, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var b = Instantiate(e.bulletPrefab, _root);
            b.hub = this;
            b.poolKey = e.key;

            if (!string.IsNullOrEmpty(e.tagOverride)) b.gameObject.tag = e.tagOverride;
            if (e.layerOverride >= 0) b.gameObject.layer = e.layerOverride;

            // 2D ��������Ʈ��� ���� ���� ����
            var sr = b.GetComponent<SpriteRenderer>();
            if (sr && !string.IsNullOrEmpty(e.sortingLayerName))
            {
                sr.sortingLayerName = e.sortingLayerName;
                sr.sortingOrder = e.orderInLayer;
            }

            b.gameObject.SetActive(false);
            stack.Push(b);
        }
    }

    public Bullet Spawn(
        BulletPoolKey key,
        Vector2 position,
        Vector2 velocity,
        float lifetime,
        float damage,
        float zRotationDeg = 0f)
    {
        var stack = _stacks[key];
        if (stack.Count == 0)
        {
            var e = _cfg[key];
            if (e.expandable) Prewarm(e, stack, e.expandBlock);
            else return null;
        }

        var b = stack.Pop();
        var tf = b.transform;
        tf.SetPositionAndRotation(position, Quaternion.Euler(0, 0, zRotationDeg));

        b.velocity = velocity;
        b.lifetime = lifetime;
        b.damage = damage;

        b.gameObject.SetActive(true);

        // Ȱ�� ��� ���
        _activeAll.Add(b);
        _activeByKey[key].Add(b);

        return b;
    }

    public void Despawn(Bullet b)
    {
        if (b == null) return;
        if (!b.gameObject.activeSelf) return;

        b.gameObject.SetActive(false);

        // Ȱ�� ��Ͽ��� ����
        _activeAll.Remove(b);
        if (_activeByKey.TryGetValue(b.poolKey, out var set))
            set.Remove(b);

        _stacks[b.poolKey].Push(b);
    }

    /// <summary>
    /// �̹� ��Ȱ��/�ı��� ������Ʈ�� �����ϰ� �����ϰ� ��ȯ.
    /// ��ź ���� �������� ���.
    /// </summary>
    void DespawnActiveUnsafe(Bullet b)
    {
        if (b == null) return;
        if (!b.gameObject.activeSelf)
        {
            // �׷��� ��Ʈ ������ �������ش�
            _activeAll.Remove(b);
            if (_activeByKey.TryGetValue(b.poolKey, out var set))
                set.Remove(b);
            return;
        }
        Despawn(b);
    }

    /// <summary>
    /// ��� ź�� �����Ѵ�.
    /// includePlayer=false �̸� �÷��̾� ź�� ����� �� ź�� ����.
    /// </summary>
    public void BombClearAll(bool includePlayer = false)
    {
        if (includePlayer)
        {
            // ��ü ���� �� �ݺ�(������ �����ϸ� ��ȸ�ϸ� ���� ����)
            var snapshot = ListFromSet(_activeAll);
            for (int i = 0; i < snapshot.Count; i++)
                DespawnActiveUnsafe(snapshot[i]);
        }
        else
        {
            // �÷��̾ ������ Ű�� ����
            foreach (var kv in _activeByKey)
            {
                if (kv.Key == BulletPoolKey.Player) continue;
                BombClear(kv.Key);
            }
        }
    }

    /// <summary>
    /// ������ Ű�� ź�� �����Ѵ�.
    /// </summary>
    public void BombClear(BulletPoolKey key)
    {
        if (!_activeByKey.TryGetValue(key, out var set) || set.Count == 0) return;

        var snapshot = ListFromSet(set);
        for (int i = 0; i < snapshot.Count; i++)
            DespawnActiveUnsafe(snapshot[i]);
    }

    /// <summary>
    /// ���� ������ �ִ� ź�� �����Ѵ�. (���� ���� ����)
    /// </summary>
    public void BombClearInRadius(Vector2 center, float radius, System.Predicate<Bullet> extraFilter = null)
    {
        float r2 = radius * radius;
        var snapshot = ListFromSet(_activeAll);

        for (int i = 0; i < snapshot.Count; i++)
        {
            var b = snapshot[i];
            if (b == null) continue;

            var pos = (Vector2)b.transform.position;
            if ((pos - center).sqrMagnitude <= r2)
            {
                if (extraFilter == null || extraFilter(b))
                    DespawnActiveUnsafe(b);
            }
        }
    }

    static List<T> ListFromSet<T>(ICollection<T> set)
    {
        // ���� ����Ʈ ��ȯ(���� �� ���� ������ ������)
        var list = new List<T>(set.Count);
        foreach (var x in set) list.Add(x);
        return list;
    }
}
