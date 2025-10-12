using UnityEngine;

[DisallowMultipleComponent]
public class Move_ZigZag : EnemyMoveBase
{
    [Header("ZigZag")]
    public float zigDownSpeed = 1.8f;
    public float zigHSpeed = 3.5f;
    public float zigInterval = 0.5f;
    public bool zigStartRight = true;

    float _dir;
    float _timer;

    protected override void OnActivated()
    {
        _dir = zigStartRight ? 1f : -1f;
        _timer = 0f;
    }

    public override float Tick(ref Vector3 pos, float dt)
    {
        _timer -= dt;
        if (_timer <= 0f)
        {
            _timer = zigInterval;
            _dir *= -1f;
            if (_dir > 0) Owner.InvokeTurnRight(); else Owner.InvokeTurnLeft();
        }

        float vx = _dir * zigHSpeed;
        pos.x += vx * dt;
        pos.y -= zigDownSpeed * dt;
        return vx;
    }
}
