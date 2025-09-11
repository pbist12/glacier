// File: BossHomingShooter.cs
using UnityEngine;

[DisallowMultipleComponent]
public class BossHomingShooter : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject homingBulletPrefab;

    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Timing")]
    [Min(0.1f)] public float coolDown = 2.0f;

    public void FireOnce()
    {
        if (!homingBulletPrefab) return;

        Vector3 pos = firePoint ? firePoint.position : transform.position;
        Quaternion rot = firePoint ? firePoint.rotation : transform.rotation;
        Instantiate(homingBulletPrefab, pos, rot);
    }
}
