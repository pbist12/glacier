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
        public string sortingLayerName; // 렌더 정리용(2D라면)
        public int orderInLayer;
        public string tagOverride;      // "PlayerBullet" / "EnemyBullet" 등
        public int layerOverride = -1;  // 0 이상이면 적용
    }

    [Header("Pools")]
    public List<Entry> entries = new();

    // 내부 상태
    readonly Dictionary<BulletPoolKey, Stack<Bullet>> _stacks = new();
    readonly Dictionary<BulletPoolKey, Entry> _cfg = new();
    Transform _root;

    void Awake()
    {
        _root = transform;
        foreach (var e in entries)
        {
            _cfg[e.key] = e;
            var stack = new Stack<Bullet>(Mathf.Max(16, e.initialCount));
            _stacks[e.key] = stack;
            Prewarm(e, stack, e.initialCount);
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

            // 2D 스프라이트라면 정렬 계층 지정
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

        b.gameObject.SetActive(true);
        return b;
    }

    public void Despawn(Bullet b)
    {
        b.gameObject.SetActive(false);
        _stacks[b.poolKey].Push(b);
    }
}
