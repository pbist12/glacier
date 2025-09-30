using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "GameMini/Boss")]
public class BossAsset : ScriptableObject
{
    public string bossName;
    public GameObject bossPrefab;
}
