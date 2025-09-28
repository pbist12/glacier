using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("Data (런타임에 ShopManager가 세팅)")]
    public RelicData itemData;
    public ShopManager owner;

    [Header("Inventory")]
    public PlayerInventory playerInventory;

    [Header("UI (월드 스페이스)")]
    public GameObject promptRoot;     // "E 구매" 프롬프트
    public SpriteRenderer sprite;     // 슬롯 위 미리보기 스프라이트

    [Header("Input (New Input System)")]
    public InputActionReference interactAction; // 반드시 Action Type=Button

    [Header("Proximity Settings")]
    public float enterRadius = 2.0f;
    public float exitRadius = 2.4f;
    public string playerTag = "Player";

    [SerializeField] private Transform player;

    // 내부 상태
    private bool _inRange;
    private InputAction _action;
    private static ShopSlot _focused;     // 가장 가까운 슬롯(전역 단일 포커스)
    private static float _focusedDistSqr;

    // ====== 주입 API ======
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
                    overrideInteractions = "press"   // 또는 "" (아무 인터랙션 없음)
                });

            action.performed += OnInteract;
        }
    }

    void OnDisable()
    {
        if (_action != null)
            _action.performed -= OnInteract;      // ★ 콜백 해제
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
        // 플레이어 참조 보정
        if (!player)
        {
            player = ResolvePlayer();
            if (!player) return;
        }

        // 거리 계산
        float sqrDist = (transform.position - player.position).sqrMagnitude;
        float enterSqr = enterRadius * enterRadius;
        float exitSqr = exitRadius * exitRadius;

        // 범위 판정 (히스테리시스)
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

        // 포커스 후보 등록: 범위 안일 때만
        if (_inRange)
        {
            // 이번 프레임의 최단 슬롯을 선정
            if (_focused == null || sqrDist < _focusedDistSqr || ReferenceEquals(_focused, this))
            {
                _focused = this;
                _focusedDistSqr = sqrDist;
                // 프롬프트 On/Off: 포커스 슬롯만 표시
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

    // ====== 콜백 기반 상호작용 ======
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        // 입력이 들어왔을 때, 내가 포커스 슬롯 & 범위 내면 처리
        if (!_inRange) return;
        if (!ReferenceEquals(_focused, this)) return;
        if (itemData == null) return;

        TryPurchase();
    }

    // ====== 내부 구현 ======
    private void EnterRange()
    {
        _inRange = true;
        ShopItemInfoUI.Instance?.ShowFor(itemData, transform); // 슬롯 따라다님
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
