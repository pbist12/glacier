using System;
using UnityEngine;

public class SceneObjectCounter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            int all = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
            int bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None).Length;

            Debug.Log($"전체 오브젝트: {all}, Bullet: {bullets}");
        }
    }
}
