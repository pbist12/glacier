using UnityEngine;

[DisallowMultipleComponent]
public class Move_HorizontalPatrol : EnemyMoveBase
{
    [Header("Horizontal Patrol")]
    public float patrolHalfWidth = 2.8f;
    public float patrolSpeed = 2.2f;
    public bool startRight = true;
    [Range(0f, 0.5f)] public float edgeEaseRatio = 0.2f;

    float _dir; // +1/-1
    Vector3 _pos;

    protected override void OnActivated()
    {
        _pos = transform.position;
        _dir = startRight ? 1f : -1f;
    }

    public override float Tick(ref Vector3 pos, float dt)
    {
        _pos = pos;

        float left = Owner.centerX - patrolHalfWidth;
        float right = Owner.centerX + patrolHalfWidth;

        float x01 = Mathf.InverseLerp(left, right, _pos.x);
        float easeEdge = 1f;
        if (edgeEaseRatio > 0f)
        {
            float edge = edgeEaseRatio;
            if (x01 < edge) easeEdge = Mathf.InverseLerp(0f, edge, x01);
            else if (x01 > 1f - edge) easeEdge = Mathf.InverseLerp(1f, 1f - edge, x01);
        }

        float vx = _dir * patrolSpeed * Mathf.Lerp(0.4f, 1f, easeEdge);
        _pos.x += vx * dt;

        if (_pos.x <= left)
        {
            _pos.x = left;
            if (_dir != 1) Owner.InvokeTurnRight();
            _dir = 1;
        }
        else if (_pos.x >= right)
        {
            _pos.x = right;
            if (_dir != -1) Owner.InvokeTurnLeft();
            _dir = -1;
        }

        pos = _pos;
        return vx;
    }
}
