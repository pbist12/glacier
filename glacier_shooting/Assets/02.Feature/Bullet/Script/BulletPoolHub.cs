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

    // 활성 탄 추적(폭탄/정리 기능용)
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

    /// <summary>
    /// 기본 Spawn: 초기 velocity/lifetime/damage 지정, 회전 zRotationDeg 적용.
    /// </summary>
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

        // 풀 입고 훅(활성화 이전에 초기화)
        if (b.TryGetComponent<IPooledBulletReset>(out var resetter))
            resetter.OnBeforeSpawnFromPool();

        b.gameObject.SetActive(true);

        // 활성 목록 등록
        _activeAll.Add(b);
        _activeByKey[key].Add(b);

        return b;
    }

    public void Despawn(Bullet b)
    {
        if (b == null) return;
        if (!b.gameObject.activeSelf) return;

        // 풀 반납 훅(비활성화 직전)
        if (b.TryGetComponent<IPooledBulletReset>(out var resetter))
            resetter.OnAfterDespawnToPool();

        b.gameObject.SetActive(false);

        // 활성 목록에서 제거
        _activeAll.Remove(b);
        if (_activeByKey.TryGetValue(b.poolKey, out var set))
            set.Remove(b);

        _stacks[b.poolKey].Push(b);
    }

    /// <summary>
    /// 이미 비활성/파괴된 오브젝트도 안전하게 무시하고 반환.
    /// 폭탄 정리 루프에서 사용.
    /// </summary>
    void DespawnActiveUnsafe(Bullet b)
    {
        if (b == null) return;
        if (!b.gameObject.activeSelf)
        {
            // 그래도 세트 정합을 보장해준다
            _activeAll.Remove(b);
            if (_activeByKey.TryGetValue(b.poolKey, out var set))
                set.Remove(b);
            return;
        }
        Despawn(b);
    }

    /// <summary>
    /// 모든 탄을 제거한다.
    /// includePlayer=false 이면 플레이어 탄은 남기고 적 탄만 제거.
    /// </summary>
    public void BombClearAll(bool includePlayer = false)
    {
        if (includePlayer)
        {
            // 전체 복사 후 반복(집합을 수정하며 순회하면 예외 가능)
            var snapshot = ListFromSet(_activeAll);
            for (int i = 0; i < snapshot.Count; i++)
                DespawnActiveUnsafe(snapshot[i]);
        }
        else
        {
            // 플레이어를 제외한 키만 정리
            foreach (var kv in _activeByKey)
            {
                if (kv.Key == BulletPoolKey.Player) continue;
                BombClear(kv.Key);
            }
        }
    }

    /// <summary>
    /// 지정한 키의 탄만 제거한다.
    /// </summary>
    public void BombClear(BulletPoolKey key)
    {
        if (!_activeByKey.TryGetValue(key, out var set) || set.Count == 0) return;

        var snapshot = ListFromSet(set);
        for (int i = 0; i < snapshot.Count; i++)
            DespawnActiveUnsafe(snapshot[i]);
    }

    /// <summary>
    /// 원형 범위에 있는 탄만 제거한다. (선택 필터 가능)
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
        // 빠른 리스트 변환(열거 중 수정 방지용 스냅샷)
        var list = new List<T>(set.Count);
        foreach (var x in set) list.Add(x);
        return list;
    }

    public int SnapshotActive(List<Bullet> buffer, bool excludePlayer = false)
    {
        buffer.Clear();
        if (excludePlayer && _activeByKey.TryGetValue(BulletPoolKey.Player, out var playerSet))
        {
            foreach (var b in _activeAll) if (b && !playerSet.Contains(b)) buffer.Add(b);
            return buffer.Count;
        }
        foreach (var b in _activeAll) if (b) buffer.Add(b);
        return buffer.Count;
    }

    // ========= 추가 오버로드/API =========

    static Vector2 DegreeToVector2(float angleDeg)
    {
        float r = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    /// <summary>
    /// (간편) 방향각 + 속도로 스폰. 가속/커브=0, damage 기본=1.
    /// </summary>
    public Bullet SpawnDir(
        BulletPoolKey key,
        Vector2 position,
        float dirDeg,
        float speed,
        float ttlSeconds,
        float damage = 1f)
    {
        Vector2 v = DegreeToVector2(dirDeg) * speed;
        var b = Spawn(key, position, v, ttlSeconds, damage, dirDeg);
        if (b == null) return null;

        if (b.TryGetComponent<IBulletKinetics>(out var kin))
        {
            Vector2 dir = DegreeToVector2(dirDeg);
            kin.Launch(dir, speed, 0f, 0f, ttlSeconds);
        }
        return b;
    }

    /// <summary>
    /// (완전) 방향/속도/가속/커브/TTL/데미지까지 한 번에.
    /// </summary>
    public Bullet SpawnFull(
        BulletPoolKey key,
        Vector2 position,
        float dirDeg,
        float speed,
        float accel,
        float curve,
        float ttlSeconds,
        float damage = 1f)
    {
        Vector2 v = DegreeToVector2(dirDeg) * speed;
        var b = Spawn(key, position, v, ttlSeconds, damage, dirDeg);
        if (b == null) return null;

        if (b.TryGetComponent<IBulletKinetics>(out var kin))
        {
            Vector2 dir = DegreeToVector2(dirDeg);
            kin.Launch(dir, speed, accel, curve, ttlSeconds);
        }
        else
        {
            // 필요 시 Bullet 타입에 추가 필드가 있다면 여기서 반영
            // 예) if (b.TryGetComponent<MyBullet>(out var mb)) { mb.accel = accel; mb.curve = curve; }
        }
        return b;
    }

    /// <summary>
    /// (고급) 커스텀 초기화 델리게이트로 어떤 탄이든 세팅.
    /// 내부 Spawn 호출 후 init 콜백에서 필요한 세팅 수행.
    /// </summary>
    public Bullet SpawnWith(
        BulletPoolKey key,
        Vector2 position,
        float zRotationDeg,
        float ttlSeconds,
        float damage,
        System.Action<Bullet> init)
    {
        var b = Spawn(key, position, Vector2.zero, ttlSeconds, damage, zRotationDeg);
        if (b == null) return null;

        init?.Invoke(b);
        return b;
    }
}

/// <summary>
/// (선택) 풀 입출고 훅: Trail/파티클/상태 초기화 등.
/// 탄 오브젝트가 구현하면 Hub가 자동 호출.
/// </summary>
public interface IPooledBulletReset
{
    void OnBeforeSpawnFromPool();   // 활성화 직전
    void OnAfterDespawnToPool();    // 비활성화 직전
}
