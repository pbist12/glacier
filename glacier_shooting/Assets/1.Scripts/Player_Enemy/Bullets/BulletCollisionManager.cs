using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

/// <summary>
/// ���� 1���� �δ� "�߾� �浹 �Ŵ���"
/// - �� ������, Ȱ�� ź���� �� ���� �о�ͼ�
///   1) �� ź -> �÷��̾ �¾Ҵ���
///   2) �÷��̾� ź -> ������ �¾Ҵ���
///   �� �����Ѵ�.
/// - ���� Ʈ����/�ݹ� ����, �Ÿ�/���� ������θ� ó���ؼ� ������.
/// </summary>
[DisallowMultipleComponent]
public class BulletCollisionManager : MonoBehaviour
{
    [Header("Refs")]
    public BulletPoolHub hub;
    public PlayerStatus player;

    [Header("Spatial Hash (�� �ĺ� ���)")]
    [Tooltip("�� �ִ� ��Ʈ �ݰ� x 2 ������ ����")]
    public float cellSize = 0.8f;

    [Header("Perf")]
    [Tooltip("�������� �˻�: ź �ε��� Ȧ/¦���� ���� ���� �����Ӹ� �˻�")]
    public bool staggerCheck = true;

    // ���� ���۵�
    readonly List<Bullet> _snapshot = new(12000);
    readonly List<EnemyHealth> _neighbors = new(64);
    SpatialHash2D _hash;
    int _parity;

    void Awake()
    {
        if (!hub) hub = FindAnyObjectByType<BulletPoolHub>();
        if (!player) player = FindAnyObjectByType<PlayerStatus>();
        _hash = new SpatialHash2D(cellSize);
    }

    void Update()
    {
        if (!hub) return;

        // 1) �� �׸��� �籸�� (EnemyHealth ����)
        _hash.Rebuild(EnemyHealth.All);

        // 2) ��� Ȱ�� ź ������
        hub.SnapshotActive(_snapshot, excludePlayer: false);

        // 3) �÷��̾� ���� ĳ��
        bool hasPlayer = player && player.gameObject.activeInHierarchy;
        Vector2 pPos = hasPlayer ? (Vector2)player.transform.position : default;
        float pRad = hasPlayer ? player.radius : 0f;

        int parity = (_parity++) & 1;

        // 4) ź ����
        for (int i = 0; i < _snapshot.Count; i++)
        {
            var b = _snapshot[i];
            if (!b || !b.gameObject.activeSelf) continue;

            if (staggerCheck && ((i & 1) != parity)) continue;

            Vector2 cur = b.transform.position;

            // A) �÷��̾� �ǰ� (�� ź��)
            if (hasPlayer && b.poolKey != BulletPoolKey.Player)
            {
                float sumR = pRad + b.hitRadius;
                if (SegmentCircle(b.prevPos, cur, pPos, sumR))
                {
                    player.OnDamaged();
                    b.Despawn();
                    continue;
                }
            }

            if (b.poolKey == BulletPoolKey.Player && BossHealth.Instance && BossHealth.Instance.gameObject.activeInHierarchy)
            {
                var boss = BossHealth.Instance;
                if (CircleOverlap(cur, b.hitRadius, (Vector2)boss.transform.position, boss.radius))
                {
                    boss.Hit(b.damage);
                    b.Despawn();
                    continue;
                }
            }

            // B) �� �ǰ� (�÷��̾� ź��)
            if (b.poolKey == BulletPoolKey.Player)
            {
                _neighbors.Clear();
                _hash.QueryNeighbors(cur, _neighbors); // ����/���� ���� �ĺ�

                for (int n = 0; n < _neighbors.Count; n++)
                {
                    var e = _neighbors[n];
                    if (!e || !e.gameObject.activeInHierarchy) continue;

                    if (CircleOverlap(cur, b.hitRadius, (Vector2)e.transform.position, e.radius))
                    {
                        e.Hit(b.damage);
                        b.Despawn();
                        break; // �� �� �¾����� ����
                    }
                }
            }
        }
    }

    #region �浹 �˻�
    // ------------------------------------------------------------------
    // �Ʒ��� "�浹 ����" ��ƿ �Լ��� (�����ϱ� ���� Ǯ� �ۼ�)
    // ------------------------------------------------------------------
    static bool CircleOverlap(Vector2 ca, float ra, Vector2 cb, float rb)
    {
        float r = ra + rb;
        return (ca - cb).sqrMagnitude <= r * r;
    }
    static bool SegmentCircle(Vector2 a, Vector2 b, Vector2 c, float r)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(c - a, ab) / (ab.sqrMagnitude + 1e-8f);
        t = Mathf.Clamp01(t);
        Vector2 p = a + ab * t;
        return (p - c).sqrMagnitude <= r * r;
    }
    #endregion
}

/// <summary>
/// 2D ������ ����(��)�� ������, ������ ���� �־� �����Ѵ�.
/// - ź�� ��ġ�� ���� ���� �ֺ� 8���� �˻��ϸ� �ǹǷ�,
///   ��ü ���� ���� �˻��ϴ� �ͺ��� �ξ� ������.
/// - Ű�� Vector2Int(���� ��ǥ)�� �����Ͽ� �������̴�.
/// </summary>
public class SpatialHash2D
{
    readonly float _cellSize;
    readonly Dictionary<Vector2Int, List<EnemyHealth>> _map = new(256);

    static readonly Vector2Int[] Neigh =
    {
        new(-1,-1), new(0,-1), new(1,-1),
        new(-1, 0), new(0, 0), new(1, 0),
        new(-1, 1), new(0, 1), new(1, 1),
    };

    public SpatialHash2D(float cellSize)
    {
        _cellSize = Mathf.Max(0.01f, cellSize);
    }

    Vector2Int ToCell(Vector2 p)
    {
        return new Vector2Int(
            Mathf.FloorToInt(p.x / _cellSize),
            Mathf.FloorToInt(p.y / _cellSize)
        );
    }

    public void Rebuild(IList<EnemyHealth> enemies)
    {
        _map.Clear();
        if (enemies == null) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (!e || !e.gameObject.activeInHierarchy) continue;

            Vector2Int k = ToCell(e.transform.position);
            if (!_map.TryGetValue(k, out var list))
            {
                list = new List<EnemyHealth>(8);
                _map[k] = list;
            }
            list.Add(e);
        }
    }

    public void QueryNeighbors(Vector2 pos, List<EnemyHealth> outList)
    {
        if (outList == null) return;

        Vector2Int c = ToCell(pos);
        for (int i = 0; i < Neigh.Length; i++)
        {
            Vector2Int k = c + Neigh[i];
            if (_map.TryGetValue(k, out var list))
                outList.AddRange(list);
        }
    }
}