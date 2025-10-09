using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Config")]
    [Min(1)] public int itemsToShow = 3;
    public ShopSlot slotPrefab;
    public Transform[] spawnPoints;
    public List<RelicData> itemPool = new();

    [Header("Input (����)")]
    public UnityEngine.InputSystem.InputActionReference interactAction; // ���Կ� ����

    public Transform playerTransform; // �ν����Ϳ� �÷��̾� ��Ʈ �巡��
    
    private readonly List<ShopSlot> _activeSlots = new();

    private void Awake()
    {
        OpenShop();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameManager.Instance.ExitShop();
        }
    }

    [ContextMenu("Open Shop")]
    public void OpenShop()
    {
        ClearShop();

        if (slotPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ShopManager ������ �ҿ����մϴ�.");
            return;
        }

        int count = Mathf.Min(itemsToShow, spawnPoints.Length);
        var picks = PickItemsUnique(count);

        for (int i = 0; i < count; i++)
        {
            var point = spawnPoints[i]; 
            
            var slot = Instantiate(slotPrefab, point.position, point.rotation, point);
            slot.Setup(picks[i], this);
            slot.SetPlayer(playerTransform);       // �� �÷��̾� Transform ����
            slot.SetInventory(GameObject.FindFirstObjectByType<PlayerInventory>()); // (����) �κ��丮 ����, ������ ���� Ž��
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

        // ��� ������ �ȸ��� ���� �ݱ� ���� ���� ����
        if (_activeSlots.Count == 0)
        {
            // ��: gameObject.SetActive(false);
        }
    }

    private List<RelicData> PickItemsUnique(int count)
    {
        var result = new List<RelicData>(count);
        if (itemPool == null || itemPool.Count == 0)
        {
            Debug.LogWarning("itemPool�� ����ֽ��ϴ�.");
            return result;
        }

        // ���� ����
        var pool = new List<RelicData>(itemPool);
        for (int i = 0; i < pool.Count; i++)
        {
            int r = Random.Range(i, pool.Count);
            (pool[i], pool[r]) = (pool[r], pool[i]);
        }

        for (int i = 0; i < count && i < pool.Count; i++)
            result.Add(pool[i]);

        return result;
    }
}
