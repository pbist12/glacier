using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ShopItemButton : MonoBehaviour
{
    private ShopManager _shop;
    private int _index;
    private Button _btn;

    [Header("UI Refs")]
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
/*
    public void Bind(ShopManager shop, int stockIndex)
    {
        _shop = shop;
        _index = stockIndex;

        if (_btn == null) _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnClick);

        UpdateVisual();
    }*/

/*    void OnClick()
    {
        if (_shop == null) return;

        if (!_shop.TryBuyIndex(_index, 1))
            Debug.Log("구매 실패: 돈/재고 부족 또는 설정 오류");
    }

    void UpdateVisual()
    {
        if (_shop == null) return;
        var s = _shop.stocks[_index];
        if (s.item == null) return;

        if (icon && s.item.icon) icon.sprite = s.item.icon;
        if (nameText) nameText.text = s.item.RelicName;

        int price = _shop.GetPrice(s);
        if (priceText) priceText.text = $"{price} G";
    }*/
}
