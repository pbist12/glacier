using UnityEngine;

[CreateAssetMenu(fileName = "BossAsset", menuName = "GameMini/Boss")]
public class BossAsset : ScriptableObject
{
    public string bossName;
    public GameObject bossPrefab;
}
