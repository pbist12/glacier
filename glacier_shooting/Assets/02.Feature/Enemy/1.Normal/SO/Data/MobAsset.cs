using UnityEngine;

[CreateAssetMenu(menuName = "Monster/MobAsset", fileName = "MobAsset")]
public class MobAsset : ScriptableObject
{
    public GameObject prefab;
    public int spawnCount = 1;
}