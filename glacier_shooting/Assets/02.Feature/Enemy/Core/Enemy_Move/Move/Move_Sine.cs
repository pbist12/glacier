using UnityEngine;

[DisallowMultipleComponent]
public class Move_Sine : EnemyMoveBase
{
    [Header("Sine")]
    public float sineDownSpeed = 1.5f;
    public float sineAmplitude = 2.0f;
    public float sineFrequency = 1.2f;

    public override float Tick(ref Vector3 pos, float dt)
    {
        float t = Owner.TimeSinceStart;
        float x = Owner.centerX + Mathf.Sin(t * Mathf.PI * 2f * sineFrequency) * sineAmplitude;
        float vx = (x - pos.x) / Mathf.Max(0.0001f, dt);
        pos.x = x;
        pos.y -= sineDownSpeed * dt;
        return vx;
    }
}
