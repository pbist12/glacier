using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EnemyMoveController : MonoBehaviour
{
    public enum MoveMode { HorizontalPatrol, Sine, ZigZag, Circle, Figure8, DashPause, PathWaypoints, SeekPlayer }

    [Header("Mode")]
    public MoveMode moveMode = MoveMode.HorizontalPatrol;

    [Header("Entry (진입 연출)")]
    public bool useEntry = true;
    public float entryTargetY = 2.8f;
    public AnimationCurve entryEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float entrySpeed = 6f;

    [Header("Center & Banking")]
    public bool lockCenterOnSpawn = true;
    public float centerX;
    public bool useBanking = true;
    [Range(0f, 45f)] public float maxBankDeg = 18f;
    public float bankLerp = 8f;

    [Header("공통 옵션")]
    public float verticalDrift = 0f;     // +면 아래로 이동(스크롤 다운 느낌이면 양수 사용)
    public bool autoDespawnBelow = true;
    public float despawnY = -6f;

    [Header("이벤트")]
    public UnityEvent onEntryComplete;
    public UnityEvent onTurnLeft;
    public UnityEvent onTurnRight;
    public UnityEvent onReachWaypoint;
    public UnityEvent onPathLoop;

    // 내부 상태
    enum Phase { Entry, Move }
    Phase _phase = Phase.Entry;
    Vector3 _spawnPos;
    Vector3 _pos;
    float _t;
    Quaternion _targetRot;

    // 모드들
    EnemyMoveBase _activeMode;
    EnemyMoveBase[] _modes;

    // --- 외부에서 모드들이 사용할 수 있는 편의 콜백/프로퍼티 (Owner 통해 접근) ---
    public Vector3 SpawnPos => _spawnPos;
    public float TimeSinceStart => _t;
    public void InvokeTurnLeft() => onTurnLeft?.Invoke();
    public void InvokeTurnRight() => onTurnRight?.Invoke();
    public void InvokeReachWaypoint() => onReachWaypoint?.Invoke();
    public void InvokePathLoop() => onPathLoop?.Invoke();

    void OnEnable()
    {
        _spawnPos = transform.position;
        _pos = _spawnPos;
        if (lockCenterOnSpawn) centerX = _pos.x;

        _phase = useEntry ? Phase.Entry : Phase.Move;
        _t = 0f;

        // 같은 오브젝트에 붙어있는 모든 모드 수집
        _modes = GetComponents<EnemyMoveBase>();

        // 선택 모드 활성화
        ActivateSelectedMode();
    }

    void ActivateSelectedMode()
    {
        _activeMode = null;
        foreach (var m in _modes)
        {
            // 이름으로 매핑 (Move_XXX 클래스명 기준)
            switch (moveMode)
            {
                case MoveMode.HorizontalPatrol: if (m is Move_HorizontalPatrol) _activeMode = m; break;
                case MoveMode.Sine: if (m is Move_Sine) _activeMode = m; break;
                case MoveMode.ZigZag: if (m is Move_ZigZag) _activeMode = m; break;
                case MoveMode.PathWaypoints: if (m is Move_PathWaypoints) _activeMode = m; break;
                case MoveMode.SeekPlayer: if (m is Move_SeekPlayer) _activeMode = m; break;
                case MoveMode.Circle: if (m.GetType().Name == "Move_Circle") _activeMode = m; break;
                case MoveMode.Figure8: if (m.GetType().Name == "Move_Figure8") _activeMode = m; break;
                case MoveMode.DashPause: if (m.GetType().Name == "Move_DashPause") _activeMode = m; break;
            }
        }

        if (_activeMode != null)
        {
            _activeMode.Initialize(this);
        }
        else
        {
            Debug.LogWarning($"[EnemyMoveController] 선택한 모드({moveMode})를 구현한 컴포넌트가 없습니다. 이 오브젝트에 해당 Move_XXX를 추가하세요.", this);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        _pos = transform.position;

        if (_phase == Phase.Entry)
        {
            // 위로 점프가 싫으면 아래 한 줄 사용:
            // float targetY = Mathf.Min(entryTargetY, _spawnPos.y);
            float targetY = entryTargetY;

            float dy = targetY - _pos.y;
            if (Mathf.Abs(dy) <= 0.01f)
            {
                _pos.y = targetY;
                ApplyPosition();
                _phase = Phase.Move;
                _t = 0f;
                onEntryComplete?.Invoke();
            }
            else
            {
                float normalized = Mathf.Clamp01(Mathf.InverseLerp(
                    0f,
                    Mathf.Max(0.01f, Mathf.Abs(targetY - _spawnPos.y)),
                    Mathf.Abs(_pos.y - _spawnPos.y)
                ));
                float eased = entryEase.Evaluate(normalized);
                float step = Mathf.Max(0.5f, entrySpeed * Mathf.Lerp(0.4f, 1f, eased)) * dt;
                _pos.y = Mathf.MoveTowards(_pos.y, targetY, step);
                ApplyPosition();
            }
            ApplyBanking(0f, dt);
            return;
        }

        _t += dt;

        float vxForBank = 0f;
        if (_activeMode != null)
        {
            vxForBank = _activeMode.Tick(ref _pos, dt);
        }

        // 공통: 수직 드리프트
        if (Mathf.Abs(verticalDrift) > 0.0001f)
            _pos.y -= verticalDrift * dt;

        ApplyPosition();
        ApplyBanking(vxForBank, dt);

        if (autoDespawnBelow && _pos.y < despawnY)
            Destroy(gameObject);
    }

    void ApplyPosition() => transform.position = _pos;

    void ApplyBanking(float vx, float dt)
    {
        if (!useBanking) return;
        float targetZ = Mathf.Clamp(-vx * 0.9f, -maxBankDeg, maxBankDeg);
        _targetRot = Quaternion.Euler(0f, 0f, targetZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRot, 1f - Mathf.Exp(-bankLerp * dt));
    }
}
