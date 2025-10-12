using UnityEngine;

[DisallowMultipleComponent]
public class Move_PathWaypoints : EnemyMoveBase
{
    [System.Serializable]
    public class Waypoint
    {
        public Vector3 position;
        public float moveSpeed = 3f;
        public float wait = 0f;
    }

    public enum PathSpace { World, RelativeToSpawn }

    [Header("Path")]
    public PathSpace pathSpace = PathSpace.RelativeToSpawn;
    public bool pathLoop = true;
    public AnimationCurve pathEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Waypoint[] waypoints;

    int _wpIndex;
    float _waitTimer;
    Vector3 _currentTargetWorld;

    protected override void OnActivated()
    {
        _wpIndex = 0;
        _waitTimer = 0f;
        if (waypoints != null && waypoints.Length > 0)
            _currentTargetWorld = ResolveWorld(waypoints[0]);
    }

    Vector3 ResolveWorld(Waypoint wp)
    {
        return (pathSpace == PathSpace.World) ? wp.position : Owner.SpawnPos + wp.position;
    }

    public override float Tick(ref Vector3 pos, float dt)
    {
        if (waypoints == null || waypoints.Length == 0) return 0f;

        if (_waitTimer > 0f)
        {
            _waitTimer -= dt;
            return 0f;
        }

        Vector3 target = _currentTargetWorld;
        Vector3 to = target - pos;
        float dist = to.magnitude;
        Waypoint wp = waypoints[_wpIndex];
        float speed = Mathf.Max(0.01f, wp.moveSpeed);

        if (dist <= speed * dt * 1.05f)
        {
            pos = target;
            Owner.InvokeReachWaypoint();

            if (wp.wait > 0f) _waitTimer = wp.wait;

            _wpIndex++;
            if (_wpIndex >= waypoints.Length)
            {
                if (pathLoop)
                {
                    _wpIndex = 0;
                    Owner.InvokePathLoop();
                }
                else
                {
                    return 0f; // ³¡
                }
            }
            _currentTargetWorld = ResolveWorld(waypoints[_wpIndex]);
            return 0f;
        }
        else
        {
            float t01 = Mathf.Clamp01(1f - (dist / (dist + speed)));
            float ease = pathEase.Evaluate(t01);
            float step = Mathf.Lerp(speed * 0.4f, speed, ease) * dt;

            Vector3 move = to.normalized * step;
            pos += move;
            return move.x / Mathf.Max(0.0001f, dt);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 basePos = Application.isPlaying ? (Owner ? Owner.SpawnPos : transform.position) : transform.position;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 w = (pathSpace == PathSpace.World) ? waypoints[i].position : basePos + waypoints[i].position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(w, 0.12f);

            if (i == 0) Gizmos.DrawLine(basePos, w);
            if (i > 0)
            {
                Vector3 prevW = (pathSpace == PathSpace.World)
                    ? waypoints[i - 1].position
                    : basePos + waypoints[i - 1].position;
                Gizmos.DrawLine(prevW, w);
            }
        }
    }
#endif
}
