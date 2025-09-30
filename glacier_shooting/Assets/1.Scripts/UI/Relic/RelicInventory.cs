using UnityEngine;

public class RelicInventory : MonoBehaviour
{
    public GameObject relicUIPrefab;
    public PlayerInventory playerInventory;

    private void Awake()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public void AddRelicUI(RelicData relic)
    {
        var a = Instantiate(relicUIPrefab,transform);
        a.GetComponent<RelicUI>().UpdateRelicImage(relic.icon);
    }

}
