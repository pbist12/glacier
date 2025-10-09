using UnityEngine;

[CreateAssetMenu(fileName = "MobData", menuName = "GameMini/Mob")]
public class MobAsset : ScriptableObject
{
    public GameObject prefab;
    public int spawnCount = 1;
}