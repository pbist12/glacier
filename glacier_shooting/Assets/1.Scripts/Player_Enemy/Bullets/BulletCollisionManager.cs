using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

/// <summary>
/// 씬에 1개만 두는 "중앙 충돌 매니저"
/// - 매 프레임, 활성 탄들을 한 번에 읽어와서
///   1) 적 탄 -> 플레이어에 맞았는지
///   2) 플레이어 탄 -> 적에게 맞았는지
///   를 판정한다.
/// - 물리 트리거/콜백 없이, 거리/기하 계산으로만 처리해서 빠르다.
/// </summary>
[DisallowMultipleComponent]
public class BulletCollisionManager : MonoBehaviour
{
    [Header("Refs")]
    public BulletPoolHub hub;
    public PlayerStatus player;

    [Header("Spatial Hash (적 후보 축소)")]
    [Tooltip("적 최대 히트 반경 x 2 정도가 무난")]
    public float cellSize = 0.8f;

    [Header("Perf")]
    [Tooltip("격프레임 검사: 탄 인덱스 홀/짝으로 나눠 절반 프레임만 검사")]
    public bool staggerCheck = true;

    // 내부 버퍼들
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

        // 1) 적 그리드 재구성 (EnemyHealth 전부)
        _hash.Rebuild(EnemyHealth.All);

        // 2) 모든 활성 탄 스냅샷
        hub.SnapshotActive(_snapshot, excludePlayer: false);

        // 3) 플레이어 정보 캐시
        bool hasPlayer = player && player.gameObject.activeInHierarchy;
        Vector2 pPos = hasPlayer ? (Vector2)player.transform.position : default;
        float pRad = hasPlayer ? player.radius : 0f;

        int parity = (_parity++) & 1;

        // 4) 탄 루프
        for (int i = 0; i < _snapshot.Count; i++)
        {
            var b = _snapshot[i];
            if (!b || !b.gameObject.activeSelf) continue;

            if (staggerCheck && ((i & 1) != parity)) continue;

            Vector2 cur = b.transform.position;

            // A) 플레이어 피격 (적 탄만)
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

            // B) 적 피격 (플레이어 탄만)
            if (b.poolKey == BulletPoolKey.Player)
            {
                _neighbors.Clear();
                _hash.QueryNeighbors(cur, _neighbors); // 같은/인접 셀만 후보

                for (int n = 0; n < _neighbors.Count; n++)
                {
                    var e = _neighbors[n];
                    if (!e || !e.gameObject.activeInHierarchy) continue;

                    if (CircleOverlap(cur, b.hitRadius, (Vector2)e.transform.position, e.radius))
                    {
                        e.Hit(b.damage);
                        b.Despawn();
                        break; // 한 번 맞았으면 종료
                    }
                }
            }
        }
    }

    #region 충돌 검사
    // ------------------------------------------------------------------
    // 아래는 "충돌 수학" 유틸 함수들 (이해하기 쉽게 풀어서 작성)
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
/// 2D 공간을 격자(셀)로 나누어, 적들을 셀에 넣어 보관한다.
/// - 탄의 위치가 속한 셀과 주변 8셀만 검사하면 되므로,
///   전체 적을 전부 검사하는 것보다 훨씬 빠르다.
/// - 키는 Vector2Int(격자 좌표)로 관리하여 직관적이다.
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