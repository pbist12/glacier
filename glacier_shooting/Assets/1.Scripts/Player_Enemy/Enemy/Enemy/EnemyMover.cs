using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public float entryTargetY = 2.8f;
    public float entrySpeed = 6f;
    public bool lockCenterOnSpawn = true;
    public float centerX;
    public float patrolHalfWidth = 2.8f;
    public float patrolSpeed = 2.2f;
    public bool startRight = true;

    private enum Phase { Entry, Patrol }
    private Phase _phase = Phase.Entry;
    private int _dir;
    private Vector3 _pos;

    void OnEnable()
    {
        _pos = transform.position;
        if (lockCenterOnSpawn) centerX = _pos.x;
        _dir = startRight ? 1 : -1;
        _phase = Phase.Entry;
    }

    void Update()
    {
        _pos = transform.position;

        if (_phase == Phase.Entry)
        {
            float step = entrySpeed * Time.deltaTime;
            _pos.y = Mathf.MoveTowards(_pos.y, entryTargetY, step);
            transform.position = _pos;

            if (Mathf.Abs(_pos.y - entryTargetY) <= 0.01f)
                _phase = Phase.Patrol;
        }
        else
        {
            _pos.x += _dir * patrolSpeed * Time.deltaTime;

            float left = centerX - patrolHalfWidth;
            float right = centerX + patrolHalfWidth;

            if (_pos.x <= left) { _pos.x = left; _dir = 1; }
            if (_pos.x >= right) { _pos.x = right; _dir = -1; }

            transform.position = _pos;
        }
    }
}
