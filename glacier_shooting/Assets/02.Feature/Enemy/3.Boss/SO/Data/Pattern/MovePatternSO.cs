using Game.Data;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

[CreateAssetMenu(menuName = "Monster/Boss/Pattern/Move", fileName = "MovePatternSO")]
public class MovePatternSO : PatternSOBase
{
    public Vector2 localOffset = new(2f, 0f);
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
#if UNITY_EDITOR
    void OnValidate() { kind = PatternKind.Move; }
#endif
}
