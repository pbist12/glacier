using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("Data (��Ÿ�ӿ� ShopManager�� ����)")]
    public RelicData itemData;
    public ShopManager owner;

    [Header("Inventory")]
    public PlayerInventory playerInventory;

    [Header("UI (���� �����̽�)")]
    public GameObject promptRoot;     // "E ����" ������Ʈ
    public SpriteRenderer sprite;     // ���� �� �̸����� ��������Ʈ

    [Header("Input (New Input System)")]
    public InputActionReference interactAction; // �ݵ�� Action Type=Button

    [Header("Proximity Settings")]
    public float enterRadius = 2.0f;
    public float exitRadius = 2.4f;
    public string playerTag = "Player";

    [SerializeField] private Transform player;

    // ���� ����
    private bool _inRange;
    private InputAction _action;
    private static ShopSlot _focused;     // ���� ����� ����(���� ���� ��Ŀ��)
    private static float _focusedDistSqr;

    // ====== ���� API ======
    public void Setup(RelicData data, ShopManager shopOwner)
    {
        itemData = data;
        owner = shopOwner;
        ApplyItemData();
    }
    public void SetPlayer(Transform t) => player = t;
    public void SetInventory(PlayerInventory inv) => playerInventory = inv;

    void OnValidate()
    {
        if (exitRadius < enterRadius)
            exitRadius = Mathf.Max(enterRadius * 1.2f, enterRadius + 0.01f);
    }

    void Start()
    {
        if (promptRoot) promptRoot.SetActive(false);
        if (!playerInventory) playerInventory = GameObject.FindFirstObjectByType<PlayerInventory>();
        if (!player) player = ResolvePlayer();
        if (itemData) ApplyItemData();
    }

    void OnEnable()
    {
        if (interactAction != null)
        {
            var action = interactAction.action;

            if (action.bindings.Count > 0)
                action.ApplyBindingOverride(0, new UnityEngine.InputSystem.InputBinding
                {
                    overrideInteractions = "press"   // �Ǵ� "" (�ƹ� ���ͷ��� ����)
                });

            action.performed += OnInteract;
        }
    }

    void OnDisable()
    {
        if (_action != null)
            _action.performed -= OnInteract;      // �� �ݹ� ����
        HideUIIfNeeded();
    }

    void OnDestroy()
    {
        if (_action != null)
            _action.performed -= OnInteract;
        HideUIIfNeeded();
    }

    void Update()
    {
        // �÷��̾� ���� ����
        if (!player)
        {
            player = ResolvePlayer();
            if (!player) return;
        }

        // �Ÿ� ���
        float sqrDist = (transform.position - player.position).sqrMagnitude;
        float enterSqr = enterRadius * enterRadius;
        float exitSqr = exitRadius * exitRadius;

        // ���� ���� (�����׸��ý�)
        if (_inRange)
        {
            if (sqrDist > exitSqr)
                LeaveRange();
        }
        else
        {
            if (sqrDist <= enterSqr)
                EnterRange();
        }

        // ��Ŀ�� �ĺ� ���: ���� ���� ����
        if (_inRange)
        {
            // �̹� �������� �ִ� ������ ����
            if (_focused == null || sqrDist < _focusedDistSqr || ReferenceEquals(_focused, this))
            {
                _focused = this;
                _focusedDistSqr = sqrDist;
                // ������Ʈ On/Off: ��Ŀ�� ���Ը� ǥ��
                if (promptRoot) promptRoot.SetActive(true);
            }
            else
            {
                if (promptRoot) promptRoot.SetActive(false);
            }
        }
        else
        {
            if (ReferenceEquals(_focused, this))
            {
                _focused = null;
            }
            if (promptRoot) promptRoot.SetActive(false);
        }
    }

    // ====== �ݹ� ��� ��ȣ�ۿ� ======
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        // �Է��� ������ ��, ���� ��Ŀ�� ���� & ���� ���� ó��
        if (!_inRange) return;
        if (!ReferenceEquals(_focused, this)) return;
        if (itemData == null) return;

        TryPurchase();
    }

    // ====== ���� ���� ======
    private void EnterRange()
    {
        _inRange = true;
        ShopItemInfoUI.Instance?.ShowFor(itemData, transform); // ���� ����ٴ�
    }

    private void LeaveRange()
    {
        _inRange = false;
        if (ReferenceEquals(_focused, this)) _focused = null;
        ShopItemInfoUI.Instance?.Hide();
        if (promptRoot) promptRoot.SetActive(false);
    }

    private void HideUIIfNeeded()
    {
        if (_inRange) ShopItemInfoUI.Instance?.Hide();
        _inRange = false;
        if (ReferenceEquals(_focused, this)) _focused = null;
        if (promptRoot) promptRoot.SetActive(false);
    }

    private void TryPurchase()
    {
        if (!playerInventory)
        {
            Debug.LogWarning("ShopSlot: PlayerInventory not found.");
            return;
        }
        if (playerInventory.gold < itemData.price)
        {
            Debug.Log("Not enough gold.");
            return;
        }

        playerInventory.AddRelicToInventory(itemData);
        playerInventory.gold -= itemData.price;

        ShopItemInfoUI.Instance?.Hide();
        if (promptRoot) promptRoot.SetActive(false);
        owner?.NotifySold(this);
        Destroy(gameObject);
    }

    private void ApplyItemData()
    {
        if (sprite && itemData) sprite.sprite = itemData.icon;
    }

    private Transform ResolvePlayer()
    {
        if (player) return player;
        var tagged = GameObject.FindGameObjectWithTag(playerTag);
        return tagged ? tagged.transform : null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, enterRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, exitRadius);
    }
}
