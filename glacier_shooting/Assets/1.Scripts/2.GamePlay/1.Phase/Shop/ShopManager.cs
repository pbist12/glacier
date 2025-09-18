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
        [Min(0)] public int priceOverride = 0;  // 0�̸� item.price ���
    }

    [Header("���� ��� (�ν����Ϳ��� ���� ����)")]
    public List<Stock> stocks = new();

    [Header("���۷���")]
    [SerializeField] private PlayerInventory playerInventory;   // �÷��̾� �κ��丮
    [SerializeField] private PlayerStatus playerStatus;         // ���� �ݿ� (IPlayerStats ����)
    [SerializeField] private GameObject shopCanvasRoot;         // ���� ��ü UI ��Ʈ
    [SerializeField] private Transform content;                 // ������ ��ư �θ�(��ũ�Ѻ� Content)
    [SerializeField] private Button itemButtonPrefab;           // ��ư ������
    [SerializeField] private Button exitButton;                 // �ݱ� ��ư

    [Header("�ɼ�")]
    public bool useOnPurchaseForOnUse = true; // OnUse ������ ���� ��� ���

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
        // ���� ��ư ����
        if (content != null)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);
        }

        // ��ư ����
        if (content == null || itemButtonPrefab == null) return;

        for (int i = 0; i < stocks.Count; i++)
        {
            var s = stocks[i];
            if (s.item == null) continue;

            var btn = Instantiate(itemButtonPrefab, content);

            // ShopItemButton ��ũ��Ʈ�� ���ε� (UI�� �����տ��� ����)
            var comp = btn.GetComponent<ShopItemButton>();
            if (comp == null) comp = btn.gameObject.AddComponent<ShopItemButton>();
            comp.Bind(this, i);
        }
    }

    // ���� ���
    public int GetPrice(Stock s)
    {
        if (s == null || s.item == null) return 0;
        if (s.priceOverride > 0) return s.priceOverride;
        return Mathf.Max(0, s.item.price);
    }

    // ���� ����
    public bool TryBuyIndex(int stockIndex, int amount = 1)
    {
        if (playerInventory == null || stockIndex < 0 || stockIndex >= stocks.Count) return false;
        var s = stocks[stockIndex];
        if (s.item == null || amount <= 0) return false;

        int priceEach = GetPrice(s);
        int total = priceEach * amount;
        if (playerInventory.gold < total) return false;

        // ����
        playerInventory.gold -= total;

        // ���� ��� ��� ó�� (�ɼ�)
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
            // �κ��丮�� �߰�
            playerInventory.AddToInventory(s.item, amount);
        }


        RefreshUI();
        return true;
    }
}
