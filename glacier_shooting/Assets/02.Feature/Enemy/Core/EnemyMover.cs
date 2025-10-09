using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EnemyMover : MonoBehaviour
{
    // ====== 공통 / 엔트리 ======
    public enum MoveMode { HorizontalPatrol, Sine, ZigZag, Circle, Figure8, DashPause, PathWaypoints }

    [Header("Mode")]
    [Tooltip("이동 패턴")]
    public MoveMode moveMode = MoveMode.HorizontalPatrol;

    [Header("Entry (진입 연출)")]
    [Tooltip("진입 연출 사용")]
    public bool useEntry = true;
    [Tooltip("진입 목표 Y")]
    public float entryTargetY = 2.8f;
    [Tooltip("진입 속도(유효 이동 최대 속도)")]
    public float entrySpeed = 6f;
    [Tooltip("진입 보간 곡선 (0~1)"), SerializeField]
    public AnimationCurve entryEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Center & Banking")]
    [Tooltip("스폰 시 X 중심 고정 (수평 패턴 기준점)")]
    public bool lockCenterOnSpawn = true;
    [Tooltip("수평 패턴들의 중앙 X")]
    public float centerX;
    [Tooltip("이동 방향에 따라 기체를 Z축으로 기울이기")]
    public bool useBanking = true;
    [Tooltip("최대 기울기(도)")]
    [Range(0f, 45f)] public float maxBankDeg = 18f;
    [Tooltip("기울기 추적 속도")]
    public float bankLerp = 8f;

    [Header("공통 드리프트/옵션")]
    [Tooltip("추가 수직 드리프트 속도(+아래)")]
    public float verticalDrift = 0f;
    [Tooltip("화면 아래로 충분히 내려가면 자동 파괴")]
    public bool autoDespawnBelow = true;
    public float despawnY = -6f;

    [Header("이벤트 (발사 타이밍 등 걸어두기)")]
    public UnityEvent onEntryComplete;
    public UnityEvent onTurnLeft;
    public UnityEvent onTurnRight;
    public UnityEvent onReachWaypoint;
    public UnityEvent onPathLoop;

    // ====== 패턴별 파라미터 ======
    [Header("Horizontal Patrol")]
    [Tooltip("좌우 반폭")]
    public float patrolHalfWidth = 2.8f;
    [Tooltip("수평 속도")]
    public float patrolSpeed = 2.2f;
    [Tooltip("오른쪽부터 시작")]
    public bool startRight = true;
    [Tooltip("끝점에서 속도를 부드럽게 줄이기(감속 영역 비율)")]
    [Range(0f, 0.5f)] public float edgeEaseRatio = 0.2f;

    [Header("Sine (사인 스네이크)")]
    [Tooltip("기본 전진(아래) 속도")]
    public float sineDownSpeed = 1.5f;
    [Tooltip("사인 진폭")]
    public float sineAmplitude = 2.0f;
    [Tooltip("사인 주파수(초당 사이클)")]
    public float sineFrequency = 1.2f;

    [Header("ZigZag")]
    [Tooltip("기본 전진(아래) 속도")]
    public float zigDownSpeed = 1.8f;
    [Tooltip("좌우 속도")]
    public float zigHSpeed = 3.5f;
    [Tooltip("방향 전환 주기(sec)")]
    public float zigInterval = 0.5f;
    [Tooltip("첫 방향이 오른쪽인지")]
    public bool zigStartRight = true;

    [Header("Circle / Figure8")]
    [Tooltip("회전 중심(비우면 스폰 위치 기준 오프셋)")]
    public Transform orbitCenter;
    [Tooltip("중심 오프셋(월드)")]
    public Vector2 centerOffset;
    [Tooltip("반지름")]
    public float radius = 2.0f;
    [Tooltip("각속도(도/초)")]
    public float angularSpeedDeg = 90f;

    [Header("Dash-Pause")]
    [Tooltip("기본 전진(아래) 속도")]
    public float dashDownSpeed = 1.2f;
    [Tooltip("대시 속도(수평)")]
    public float dashSpeed = 7f;
    [Tooltip("대시 유지 시간")]
    public float dashDuration = 0.35f;
    [Tooltip("대시 후 정지 시간")]
    public float pauseDuration = 0.25f;
    [Tooltip("오른쪽부터 대시 시작")]
    public bool dashStartRight = true;

    [System.Serializable]
    public class Waypoint
    {
        [Tooltip("웨이포인트 위치(월드/상대 선택)")]
        public Vector3 position;
        [Tooltip("이 지점까지의 이동 속도")]
        public float moveSpeed = 3f;
        [Tooltip("도착 시 대기 시간")]
        public float wait = 0f;
    }

    public enum PathSpace { World, RelativeToSpawn }

    [Header("Path (Waypoints)")]
    [Tooltip("좌표계")]
    public PathSpace pathSpace = PathSpace.RelativeToSpawn;
    [Tooltip("한 번 끝나면 반복할지")]
    public bool pathLoop = true;
    [Tooltip("웨이포인트 간 보간 곡선(0~1)")]
    public AnimationCurve pathEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Waypoint[] waypoints;

    // ====== 내부 상태 ======
    private enum Phase { Entry, Move }
    private Phase _phase = Phase.Entry;
    private Vector3 _spawnPos;
    private Vector3 _pos;
    private float _t;                 // 시간 누적(패턴별 사용)
    private int _dir;                 // 좌우(+1 / -1)
    private float _zigTimer;
    private float _dashTimer;
    private bool _isDashing;
    private int _wpIndex;
    private Vector3 _pathTargetWorld;
    private float _pathWaitTimer;
    private Quaternion _targetRot;

    void OnEnable()
    {
        _spawnPos = transform.position;
        _pos = _spawnPos;
        if (lockCenterOnSpawn) centerX = _pos.x;

        _phase = useEntry ? Phase.Entry : Phase.Move;
        _t = 0f;

        // 초기 방향
        _dir = startRight ? 1 : -1;
        if (moveMode == MoveMode.ZigZag) _dir = (zigStartRight ? 1 : -1);
        if (moveMode == MoveMode.DashPause) { _dir = dashStartRight ? 1 : -1; _isDashing = true; _dashTimer = dashDuration; }

        // 경로 초기화
        _wpIndex = 0;
        _pathWaitTimer = 0f;
        if (moveMode == MoveMode.PathWaypoints && waypoints != null && waypoints.Length > 0)
            _pathTargetWorld = ResolveWaypointWorld(waypoints[0]);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        _pos = transform.position;

        if (_phase == Phase.Entry)
        {
            // 간단한 y MoveTowards + 이징
            float dy = entryTargetY - _pos.y;
            float dist = Mathf.Abs(dy);
            if (dist <= 0.01f)
            {
                _pos.y = entryTargetY;
                transform.position = _pos;
                _phase = Phase.Move;
                _t = 0f;
                onEntryComplete?.Invoke();
            }
            else
            {
                // MoveTowards와 ease를 섞어 자연스럽게
                float normalized = Mathf.Clamp01(Mathf.InverseLerp(0f, Mathf.Max(0.01f, Mathf.Abs(entryTargetY - _spawnPos.y)), Mathf.Abs(_pos.y - _spawnPos.y)));
                float eased = entryEase.Evaluate(normalized);
                float step = Mathf.Max(0.5f, entrySpeed * Mathf.Lerp(0.4f, 1f, eased)) * dt;
                _pos.y = Mathf.MoveTowards(_pos.y, entryTargetY, step);
                transform.position = _pos;
            }
            ApplyBanking(0f, dt);
            return;
        }

        // ===== Move Phase =====
        _t += dt;

        switch (moveMode)
        {
            case MoveMode.HorizontalPatrol:
                UpdateHorizontalPatrol(dt);
                break;

            case MoveMode.Sine:
                UpdateSine(dt);
                break;

            case MoveMode.ZigZag:
                UpdateZigZag(dt);
                break;

            case MoveMode.Circle:
                UpdateCircle(dt);
                break;

            case MoveMode.Figure8:
                UpdateFigure8(dt);
                break;

            case MoveMode.DashPause:
                UpdateDashPause(dt);
                break;

            case MoveMode.PathWaypoints:
                UpdatePath(dt);
                break;
        }

        // 공통 수직 드리프트
        if (Mathf.Abs(verticalDrift) > 0.0001f)
        {
            _pos.y -= verticalDrift * dt;
        }

        transform.position = _pos;

        if (autoDespawnBelow && _pos.y < despawnY)
            Destroy(gameObject);
    }

    // ===== 패턴 구현 =====

    void UpdateHorizontalPatrol(float dt)
    {
        float left = centerX - patrolHalfWidth;
        float right = centerX + patrolHalfWidth;

        // 끝점 근처에서 감속 (edgeEaseRatio 비율만큼)
        float range = patrolHalfWidth * 2f;
        float x01 = Mathf.InverseLerp(left, right, _pos.x);
        float easeEdge = 1f;
        if (edgeEaseRatio > 0f)
        {
            float edge = edgeEaseRatio;
            if (x01 < edge) easeEdge = Mathf.InverseLerp(0f, edge, x01); // 왼쪽
            else if (x01 > 1f - edge) easeEdge = Mathf.InverseLerp(1f, 1f - edge, x01); // 오른쪽
        }

        float vx = _dir * patrolSpeed * Mathf.Lerp(0.4f, 1f, easeEdge);
        _pos.x += vx * dt;

        if (_pos.x <= left)
        {
            _pos.x = left;
            if (_dir != 1) onTurnRight?.Invoke();
            _dir = 1;
        }
        else if (_pos.x >= right)
        {
            _pos.x = right;
            if (_dir != -1) onTurnLeft?.Invoke();
            _dir = -1;
        }

        ApplyBanking(vx, dt);
    }

    void UpdateSine(float dt)
    {
        float x = centerX + Mathf.Sin(_t * Mathf.PI * 2f * sineFrequency) * sineAmplitude;
        float vx = (x - _pos.x) / Mathf.Max(0.0001f, dt);
        _pos.x = x;
        _pos.y -= sineDownSpeed * dt;
        ApplyBanking(vx, dt);
    }

    void UpdateZigZag(float dt)
    {
        _zigTimer -= dt;
        if (_zigTimer <= 0f)
        {
            _zigTimer = zigInterval;
            _dir *= -1;
            if (_dir > 0) onTurnRight?.Invoke(); else onTurnLeft?.Invoke();
        }

        float vx = _dir * zigHSpeed;
        _pos.x += vx * dt;
        _pos.y -= zigDownSpeed * dt;

        ApplyBanking(vx, dt);
    }

    void UpdateCircle(float dt)
    {
        Vector3 c = GetOrbitCenter();
        float ang = (_t * angularSpeedDeg) * Mathf.Deg2Rad;
        float x = c.x + Mathf.Cos(ang) * radius;
        float y = c.y + Mathf.Sin(ang) * radius;

        float vx = (x - _pos.x) / Mathf.Max(0.0001f, dt);
        _pos.x = x;
        _pos.y = y;

        ApplyBanking(vx, dt);
    }

    void UpdateFigure8(float dt)
    {
        // 라사주(2:1) 형태로 피겨8
        Vector3 c = GetOrbitCenter();
        float ang = (_t * angularSpeedDeg) * Mathf.Deg2Rad;
        float x = c.x + Mathf.Sin(ang * 2f) * radius;
        float y = c.y + Mathf.Sin(ang) * radius * 0.7f; // 살짝 납작하게

        float vx = (x - _pos.x) / Mathf.Max(0.0001f, dt);
        _pos.x = x;
        _pos.y = y;

        ApplyBanking(vx, dt);
    }

    void UpdateDashPause(float dt)
    {
        float vx = 0f;

        if (_isDashing)
        {
            vx = _dir * dashSpeed;
            _pos.x += vx * dt;
            _pos.y -= dashDownSpeed * dt;

            _dashTimer -= dt;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _dashTimer = pauseDuration;
                // 대시 종료(멈춤 시작)
            }
        }
        else
        {
            // 멈춤
            _pos.y -= dashDownSpeed * 0.4f * dt;
            _dashTimer -= dt;
            if (_dashTimer <= 0f)
            {
                _isDashing = true;
                _dashTimer = dashDuration;
                _dir *= -1;
                if (_dir > 0) onTurnRight?.Invoke(); else onTurnLeft?.Invoke();
            }
        }

        ApplyBanking(vx, dt);
    }

    void UpdatePath(float dt)
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 target = _pathTargetWorld;
        Vector3 to = target - _pos;
        float dist = to.magnitude;
        Waypoint wp = waypoints[_wpIndex];
        float speed = Mathf.Max(0.01f, wp.moveSpeed);

        if (dist <= speed * dt * 1.05f)
        {
            // 도착 처리
            _pos = target;
            transform.position = _pos;
            onReachWaypoint?.Invoke();

            if (wp.wait > 0f)
            {
                _pathWaitTimer = wp.wait;
                // 대기 중에는 그대로 둔다
            }

            // 다음 인덱스
            if (_pathWaitTimer <= 0f)
            {
                _wpIndex++;
                if (_wpIndex >= waypoints.Length)
                {
                    if (pathLoop)
                    {
                        _wpIndex = 0;
                        onPathLoop?.Invoke();
                    }
                    else
                    {
                        // 경로 끝났으면 가만히
                        return;
                    }
                }
                _pathTargetWorld = ResolveWaypointWorld(waypoints[_wpIndex]);
            }
        }
        else
        {
            if (_pathWaitTimer > 0f)
            {
                _pathWaitTimer -= dt;
                return;
            }

            // 가속/감속 느낌을 주기 위해 거리 기반 이징 가중치
            float total = Mathf.Max(dist, 0.001f);
            float t01 = Mathf.Clamp01(1f - (dist / (dist + speed))); // 근사 normalized
            float ease = pathEase.Evaluate(t01);
            float step = Mathf.Lerp(speed * 0.4f, speed, ease) * dt;

            Vector3 move = to.normalized * step;
            _pos += move;

            ApplyBanking(move.x / Mathf.Max(0.0001f, dt), dt);
        }
    }

    // ===== 헬퍼 =====

    Vector3 GetOrbitCenter()
    {
        Vector3 c = (orbitCenter != null) ? orbitCenter.position : _spawnPos;
        c += (Vector3)centerOffset;
        return c;
    }

    Vector3 ResolveWaypointWorld(Waypoint wp)
    {
        return (pathSpace == PathSpace.World) ? wp.position : _spawnPos + wp.position;
    }

    void ApplyBanking(float vx, float dt)
    {
        if (!useBanking) return;
        float targetZ = Mathf.Clamp(-vx * 0.9f, -maxBankDeg, maxBankDeg); // 오른쪽 이동 시 우측으로 기울기
        _targetRot = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRot, 1f - Mathf.Exp(-bankLerp * dt));
    }

    // ===== 기즈모 =====
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // Patrol 폭
        if (moveMode == MoveMode.HorizontalPatrol)
        {
            float left = (lockCenterOnSpawn ? centerX : transform.position.x) - patrolHalfWidth;
            float right = (lockCenterOnSpawn ? centerX : transform.position.x) + patrolHalfWidth;
            Vector3 l = new Vector3(left, transform.position.y, 0f);
            Vector3 r = new Vector3(right, transform.position.y, 0f);
            Gizmos.DrawLine(l + Vector3.up * 0.3f, l + Vector3.down * 0.3f);
            Gizmos.DrawLine(r + Vector3.up * 0.3f, r + Vector3.down * 0.3f);
            Gizmos.DrawLine(l, r);
        }

        // 원/피겨8 중심 & 반지름
        if (moveMode == MoveMode.Circle || moveMode == MoveMode.Figure8)
        {
            Vector3 c = Application.isPlaying ? GetOrbitCenter() :
                         ((orbitCenter != null) ? orbitCenter.position : transform.position) + (Vector3)centerOffset;
            Gizmos.DrawWireSphere(c, radius);
        }

        // 웨이포인트
        if (moveMode == MoveMode.PathWaypoints && waypoints != null && waypoints.Length > 0)
        {
            Vector3 prev = Application.isPlaying ? _spawnPos : transform.position;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector3 w = (pathSpace == PathSpace.World)
                    ? waypoints[i].position
                    : (Application.isPlaying ? _spawnPos : transform.position) + waypoints[i].position;

                Gizmos.DrawWireSphere(w, 0.12f);
                if (i == 0) Gizmos.DrawLine(prev, w);
                if (i > 0)
                {
                    Vector3 prevW = (pathSpace == PathSpace.World)
                        ? waypoints[i - 1].position
                        : (Application.isPlaying ? _spawnPos : transform.position) + waypoints[i - 1].position;
                    Gizmos.DrawLine(prevW, w);
                }
            }
        }
    }
}
