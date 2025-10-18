using UnityEngine;

public class NthShotStyling : MonoBehaviour
{
    [Min(1)] public int nth = 4;
    public Color color = Color.red;
    public bool enabledByEffect = false;

    public bool IsNth(int count) => enabledByEffect && nth > 0 && (count % nth) == 0;
}