using UnityEngine;
using UnityEngine.InputSystem;

public class KeyGhostTest : MonoBehaviour
{
    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool fireX = kb[Key.X]?.isPressed ?? false;
        bool a = kb[Key.A]?.isPressed ?? false;
        bool s = kb[Key.S]?.isPressed ?? false;
        bool w = kb[Key.W]?.isPressed ?? false;
        bool d = kb[Key.D]?.isPressed ?? false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"SPACE down. X:{fireX}  A:{a} S:{s} W:{w} D:{d}");
        }
    }
}
