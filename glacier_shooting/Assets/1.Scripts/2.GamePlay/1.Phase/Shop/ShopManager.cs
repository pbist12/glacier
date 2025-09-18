using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class Stock
    {
        public ItemData item;
        [Min(0)] public int priceOverride = 0;  // 0이면 item.price 사용
    }

    [Header("상점 재고 (인스펙터에서 직접 설정)")]
    public List<Stock> stocks = new();

    [Header("레퍼런스")]
    [SerializeField] private PlayerInventory playerInventory;   // 플레이어 인벤토리
    [SerializeField] private PlayerStatus playerStatus;         // 스탯 반영 (IPlayerStats 구현)
    [SerializeField] private GameObject shopCanvasRoot;         // 상점 전체 UI 루트
    [SerializeField] private Transform content;                 // 아이템 버튼 부모(스크롤뷰 Content)
    [SerializeField] private Button itemButtonPrefab;           // 버튼 프리팹
    [SerializeField] private Button exitButton;                 // 닫기 버튼

    [Header("옵션")]
    public bool useOnPurchaseForOnUse = true; // OnUse 아이템 구매 즉시 사용

    void Awake()
    {
        if (exitButton != null) exitButton.onClick.AddListener(Exit);
        if (shopCanvasRoot != null) shopCanvasRoot.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && shopCanvasRoot.activeSelf)
        {
            Exit();
        }
    }

    public void Open()
    {
        if (shopCanvasRoot != null) shopCanvasRoot.SetActive(true);
        RefreshUI();
    }

    public void Close()
    {
        if (shopCanvasRoot != null) shopCanvasRoot.SetActive(false);
    }

    void Exit()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ExitShop();
        else
            Close();
    }

    public void RefreshUI()
    {
        // 기존 버튼 제거
        if (content != null)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);
        }

        // 버튼 생성
        if (content == null || itemButtonPrefab == null) return;

        for (int i = 0; i < stocks.Count; i++)
        {
            var s = stocks[i];
            if (s.item == null) continue;

            var btn = Instantiate(itemButtonPrefab, content);

            // ShopItemButton 스크립트로 바인드 (UI는 프리팹에서 세팅)
            var comp = btn.GetComponent<ShopItemButton>();
            if (comp == null) comp = btn.gameObject.AddComponent<ShopItemButton>();
            comp.Bind(this, i);
        }
    }

    // 가격 계산
    public int GetPrice(Stock s)
    {
        if (s == null || s.item == null) return 0;
        if (s.priceOverride > 0) return s.priceOverride;
        return Mathf.Max(0, s.item.price);
    }

    // 구매 로직
    public bool TryBuyIndex(int stockIndex, int amount = 1)
    {
        if (playerInventory == null || stockIndex < 0 || stockIndex >= stocks.Count) return false;
        var s = stocks[stockIndex];
        if (s.item == null || amount <= 0) return false;

        int priceEach = GetPrice(s);
        int total = priceEach * amount;
        if (playerInventory.gold < total) return false;

        // 결제
        playerInventory.gold -= total;

        // 구매 즉시 사용 처리 (옵션)
        bool consumedAll = false;
        if (useOnPurchaseForOnUse && s.item.useMode == UseMode.OnUse)
        {
            var ctx = new ItemContext(
                owner: playerStatus.gameObject,
                inventory: playerInventory,
                stats: playerStatus,
                logger: Debug.Log
            );

            for (int i = 0; i < amount; i++)
                ItemRuntime.Use(s.item, ctx);

            playerStatus?.SetStat();
            consumedAll = true;
        }

        if (!consumedAll)
        {
            // 인벤토리에 추가
            playerInventory.AddToInventory(s.item, amount);
        }


        RefreshUI();
        return true;
    }
}
