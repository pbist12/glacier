using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EnemyMover : MonoBehaviour
{
    // ====== ���� / ��Ʈ�� ======
    public enum MoveMode { HorizontalPatrol, Sine, ZigZag, Circle, Figure8, DashPause, PathWaypoints }

    [Header("Mode")]
    [Tooltip("�̵� ����")]
    public MoveMode moveMode = MoveMode.HorizontalPatrol;

    [Header("Entry (���� ����)")]
    [Tooltip("���� ���� ���")]
    public bool useEntry = true;
    [Tooltip("���� ��ǥ Y")]
    public float entryTargetY = 2.8f;
    [Tooltip("���� �ӵ�(��ȿ �̵� �ִ� �ӵ�)")]
    public float entrySpeed = 6f;
    [Tooltip("���� ���� � (0~1)"), SerializeField]
    public AnimationCurve entryEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Center & Banking")]
    [Tooltip("���� �� X �߽� ���� (���� ���� ������)")]
    public bool lockCenterOnSpawn = true;
    [Tooltip("���� ���ϵ��� �߾� X")]
    public float centerX;
    [Tooltip("�̵� ���⿡ ���� ��ü�� Z������ ����̱�")]
    public bool useBanking = true;
    [Tooltip("�ִ� ����(��)")]
    [Range(0f, 45f)] public float maxBankDeg = 18f;
    [Tooltip("���� ���� �ӵ�")]
    public float bankLerp = 8f;

    [Header("���� �帮��Ʈ/�ɼ�")]
    [Tooltip("�߰� ���� �帮��Ʈ �ӵ�(+�Ʒ�)")]
    public float verticalDrift = 0f;
    [Tooltip("ȭ�� �Ʒ��� ����� �������� �ڵ� �ı�")]
    public bool autoDespawnBelow = true;
    public float despawnY = -6f;

    [Header("�̺�Ʈ (�߻� Ÿ�̹� �� �ɾ�α�)")]
    public UnityEvent onEntryComplete;
    public UnityEvent onTurnLeft;
    public UnityEvent onTurnRight;
    public UnityEvent onReachWaypoint;
    public UnityEvent onPathLoop;

    // ====== ���Ϻ� �Ķ���� ======
    [Header("Horizontal Patrol")]
    [Tooltip("�¿� ����")]
    public float patrolHalfWidth = 2.8f;
    [Tooltip("���� �ӵ�")]
    public float patrolSpeed = 2.2f;
    [Tooltip("�����ʺ��� ����")]
    public bool startRight = true;
    [Tooltip("�������� �ӵ��� �ε巴�� ���̱�(���� ���� ����)")]
    [Range(0f, 0.5f)] public float edgeEaseRatio = 0.2f;

    [Header("Sine (���� ������ũ)")]
    [Tooltip("�⺻ ����(�Ʒ�) �ӵ�")]
    public float sineDownSpeed = 1.5f;
    [Tooltip("���� ����")]
    public float sineAmplitude = 2.0f;
    [Tooltip("���� ���ļ�(�ʴ� ����Ŭ)")]
    public float sineFrequency = 1.2f;

    [Header("ZigZag")]
    [Tooltip("�⺻ ����(�Ʒ�) �ӵ�")]
    public float zigDownSpeed = 1.8f;
    [Tooltip("�¿� �ӵ�")]
    public float zigHSpeed = 3.5f;
    [Tooltip("���� ��ȯ �ֱ�(sec)")]
    public float zigInterval = 0.5f;
    [Tooltip("ù ������ ����������")]
    public bool zigStartRight = true;

    [Header("Circle / Figure8")]
    [Tooltip("ȸ�� �߽�(���� ���� ��ġ ���� ������)")]
    public Transform orbitCenter;
    [Tooltip("�߽� ������(����)")]
    public Vector2 centerOffset;
    [Tooltip("������")]
    public float radius = 2.0f;
    [Tooltip("���ӵ�(��/��)")]
    public float angularSpeedDeg = 90f;

    [Header("Dash-Pause")]
    [Tooltip("�⺻ ����(�Ʒ�) �ӵ�")]
    public float dashDownSpeed = 1.2f;
    [Tooltip("��� �ӵ�(����)")]
    public float dashSpeed = 7f;
    [Tooltip("��� ���� �ð�")]
    public float dashDuration = 0.35f;
    [Tooltip("��� �� ���� �ð�")]
    public float pauseDuration = 0.25f;
    [Tooltip("�����ʺ��� ��� ����")]
    public bool dashStartRight = true;

    [System.Serializable]
    public class Waypoint
    {
        [Tooltip("��������Ʈ ��ġ(����/��� ����)")]
        public Vector3 position;
        [Tooltip("�� ���������� �̵� �ӵ�")]
        public float moveSpeed = 3f;
        [Tooltip("���� �� ��� �ð�")]
        public float wait = 0f;
    }

    public enum PathSpace { World, RelativeToSpawn }

    [Header("Path (Waypoints)")]
    [Tooltip("��ǥ��")]
    public PathSpace pathSpace = PathSpace.RelativeToSpawn;
    [Tooltip("�� �� ������ �ݺ�����")]
    public bool pathLoop = true;
    [Tooltip("��������Ʈ �� ���� �(0~1)")]
    public AnimationCurve pathEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Waypoint[] waypoints;

    // ====== ���� ���� ======
    private enum Phase { Entry, Move }
    private Phase _phase = Phase.Entry;
    private Vector3 _spawnPos;
    private Vector3 _pos;
    private float _t;                 // �ð� ����(���Ϻ� ���)
    private int _dir;                 // �¿�(+1 / -1)
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

        // �ʱ� ����
        _dir = startRight ? 1 : -1;
        if (moveMode == MoveMode.ZigZag) _dir = (zigStartRight ? 1 : -1);
        if (moveMode == MoveMode.DashPause) { _dir = dashStartRight ? 1 : -1; _isDashing = true; _dashTimer = dashDuration; }

        // ��� �ʱ�ȭ
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
            // ������ y MoveTowards + ��¡
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
                // MoveTowards�� ease�� ���� �ڿ�������
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

        // ���� ���� �帮��Ʈ
        if (Mathf.Abs(verticalDrift) > 0.0001f)
        {
            _pos.y -= verticalDrift * dt;
        }

        transform.position = _pos;

        if (autoDespawnBelow && _pos.y < despawnY)
            Destroy(gameObject);
    }

    // ===== ���� ���� =====

    void UpdateHorizontalPatrol(float dt)
    {
        float left = centerX - patrolHalfWidth;
        float right = centerX + patrolHalfWidth;

        // ���� ��ó���� ���� (edgeEaseRatio ������ŭ)
        float range = patrolHalfWidth * 2f;
        float x01 = Mathf.InverseLerp(left, right, _pos.x);
        float easeEdge = 1f;
        if (edgeEaseRatio > 0f)
        {
            float edge = edgeEaseRatio;
            if (x01 < edge) easeEdge = Mathf.InverseLerp(0f, edge, x01); // ����
            else if (x01 > 1f - edge) easeEdge = Mathf.InverseLerp(1f, 1f - edge, x01); // ������
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
        // �����(2:1) ���·� �ǰ�8
        Vector3 c = GetOrbitCenter();
        float ang = (_t * angularSpeedDeg) * Mathf.Deg2Rad;
        float x = c.x + Mathf.Sin(ang * 2f) * radius;
        float y = c.y + Mathf.Sin(ang) * radius * 0.7f; // ��¦ �����ϰ�

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
                // ��� ����(���� ����)
            }
        }
        else
        {
            // ����
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
            // ���� ó��
            _pos = target;
            transform.position = _pos;
            onReachWaypoint?.Invoke();

            if (wp.wait > 0f)
            {
                _pathWaitTimer = wp.wait;
                // ��� �߿��� �״�� �д�
            }

            // ���� �ε���
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
                        // ��� �������� ������
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

            // ����/���� ������ �ֱ� ���� �Ÿ� ��� ��¡ ����ġ
            float total = Mathf.Max(dist, 0.001f);
            float t01 = Mathf.Clamp01(1f - (dist / (dist + speed))); // �ٻ� normalized
            float ease = pathEase.Evaluate(t01);
            float step = Mathf.Lerp(speed * 0.4f, speed, ease) * dt;

            Vector3 move = to.normalized * step;
            _pos += move;

            ApplyBanking(move.x / Mathf.Max(0.0001f, dt), dt);
        }
    }

    // ===== ���� =====

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
        float targetZ = Mathf.Clamp(-vx * 0.9f, -maxBankDeg, maxBankDeg); // ������ �̵� �� �������� ����
        _targetRot = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRot, 1f - Mathf.Exp(-bankLerp * dt));
    }

    // ===== ����� =====
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // Patrol ��
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

        // ��/�ǰ�8 �߽� & ������
        if (moveMode == MoveMode.Circle || moveMode == MoveMode.Figure8)
        {
            Vector3 c = Application.isPlaying ? GetOrbitCenter() :
                         ((orbitCenter != null) ? orbitCenter.position : transform.position) + (Vector3)centerOffset;
            Gizmos.DrawWireSphere(c, radius);
        }

        // ��������Ʈ
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
