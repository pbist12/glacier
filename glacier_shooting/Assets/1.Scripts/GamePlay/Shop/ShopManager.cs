using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;

    public void Open()
    {
        if (shopUI) shopUI.SetActive(true);
        // TODO: ���� ������/��� ����
    }

    public void Close()
    {
        if (shopUI) shopUI.SetActive(false);
    }
}
