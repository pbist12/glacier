using UnityEngine;

public class SceneObjectCounter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            int all = FindObjectsOfType<GameObject>().Length;
            int bullets = FindObjectsOfType<Bullet>().Length;

            Debug.Log($"전체 오브젝트: {all}, Bullet: {bullets}");
        }
    }
}
