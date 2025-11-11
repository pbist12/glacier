using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    [Header("Config")]
    [Min(1)] public int itemsToShow = 3;
    public ShopSlot slotPrefab;
    public Transform[] spawnPoints;
    public RelicDatabase itemPool;

    [Header("Input (전달)")]
    public InputActionReference interactAction; // 슬롯에 전달
    public InputActionReference quitAction;

    public Transform playerTransform; // 인스펙터에 플레이어 루트 드래그
    
    private readonly List<ShopSlot> _activeSlots = new();

    private void Awake()
    {
        OpenShop();
    }

    private void Update()
    {
        if (quitAction.action.WasPressedThisFrame())
        {
            Debug.Log("상점 나가기");
            GameManager.Instance.ExitShop();
        }
    }

    [ContextMenu("Open Shop")]
    public void OpenShop()
    {
        ClearShop();

        if (slotPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ShopManager 설정이 불완전합니다.");
            return;
        }

        int count = Mathf.Min(itemsToShow, spawnPoints.Length);
        var picks = PickItemsUnique(count);

        for (int i = 0; i < count; i++)
        {
            var point = spawnPoints[i]; 
            
            var slot = Instantiate(slotPrefab, point.position, point.rotation, point);
            slot.Setup(picks[i], this);
            slot.SetPlayer(playerTransform);       // ★ 플레이어 Transform 주입
            slot.SetInventory(GameObject.FindFirstObjectByType<PlayerInventory>()); // (선택) 인벤토리 주입, 없으면 전역 탐색
            if (interactAction) slot.interactAction = interactAction;
            
            _activeSlots.Add(slot);
        }
    }

    [ContextMenu("Clear Shop")]
    public void ClearShop()
    {
        for (int i = _activeSlots.Count - 1; i >= 0; i--)
        {
            if (_activeSlots[i]) Destroy(_activeSlots[i].gameObject);
        }
        _activeSlots.Clear();
    }

    public void NotifySold(ShopSlot slot)
    {
        _activeSlots.Remove(slot);

        // 모든 슬롯이 팔리면 상점 닫기 같은 연출 가능
        if (_activeSlots.Count == 0)
        {
            // 예: gameObject.SetActive(false);
        }
    }

    private List<RelicData> PickItemsUnique(int count)
    {
        var result = new List<RelicData>(count);
        if (itemPool == null || itemPool.Count == 0)
        {
            Debug.LogWarning("itemPool이 비어있습니다.");
            return result;
        }

        // 간단 셔플
        var pool = itemPool;
        for (int i = 0; i < pool.relics.Count; i++)
        {
            int r = Random.Range(i, pool.Count);
            (pool.relics[i], pool.relics[r]) = (pool.relics[r], pool.relics[i]);
        }

        for (int i = 0; i < count && i < pool.Count; i++)
            result.Add(pool.relics[i]);

        return result;
    }
}
