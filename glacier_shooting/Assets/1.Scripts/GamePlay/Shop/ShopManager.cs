using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;

    public void Open()
    {
        if (shopUI) shopUI.SetActive(true);
        // TODO: 상점 아이템/골드 로직
    }

    public void Close()
    {
        if (shopUI) shopUI.SetActive(false);
    }
}
