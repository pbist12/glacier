using Game.Data;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

[CreateAssetMenu(menuName = "Monster/Boss/Pattern/Laser", fileName = "LaserPatternSO")]
public class LaserPatternSO : PatternSOBase
{
    public float chargeSeconds = 0.6f; // �߰� �ڷ��׷��� ����
    public float beamWidth = 0.5f;
    public float sweepDegPerSec = 0f;  // 0�̸� ������
#if UNITY_EDITOR
    void OnValidate() { kind = PatternKind.Laser; }
#endif
}
