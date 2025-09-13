using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    public List<GameObject> dropPrefab;

    public void DropItem()
    {
        Instantiate(dropPrefab[Random.Range(0, dropPrefab.Count)], transform.position, Quaternion.identity, null);
    }
}
