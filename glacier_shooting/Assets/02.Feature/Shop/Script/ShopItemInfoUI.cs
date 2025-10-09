// File: ShopItemInfoUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemInfoUI : MonoBehaviour
{
    [Header("Refs (�ڵ� ���ε� ����)")]
    public CanvasGroup canvasGroup;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;

    [Header("Follow (���� �����̽����� ������ ����ٴ�)")]
    public bool followTarget = true;
    public Transform target;                // ���� ���(���� ShopSlot.transform)
    public Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Behavior")]
    [Min(0f)] public float fadeDuration = 0.12f;
    public string priceSuffix = " G";

    private float _alphaVel;
    private float _targetAlpha = 0f;

    #region Singleton (����)
    public static ShopItemInfoUI Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // ���� ���ε�
        if (!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        // ����ڰ� �÷��� ��� ��Ī�� ���� �ڵ� ã�� (������ �ν����� ���� ����)
        if (!iconImage)
            iconImage = transform.Find("Image/Image_Item")?.GetComponent<Image>();
        if (!nameText)
            nameText = transform.Find("Image/Text_Item_Name")?.GetComponent<TextMeshProUGUI>();
        if (!priceText)
            priceText = transform.Find("Image/Text_Item_Price")?.GetComponent<TextMeshProUGUI>();
        if (!descriptionText)
            descriptionText = transform.Find("Image/Text_Item_Desc")?.GetComponent<TextMeshProUGUI>();

        // �ʱ� �񰡽�ȭ
        SetAlphaImmediate(0f);
    }
    #endregion

    void LateUpdate()
    {
        // �ε巯�� ���̵�
        if (canvasGroup)
        {
            // SmoothDamp ��� MoveTowards�� ������
            if (!Mathf.Approximately(canvasGroup.alpha, _targetAlpha))
            {
                float step = (fadeDuration > 0f)
                    ? Time.deltaTime / fadeDuration
                    : 1f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, step);
                canvasGroup.interactable = canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.99f;
            }
        }

        // Ÿ�� ���󰡱�
        if (followTarget && target)
        {
            transform.position = target.position + worldOffset;
            // �ʿ��ϸ� ī�޶� ������:
            if (Camera.main) transform.forward = Camera.main.transform.forward;
        }
    }

    public void ShowFor(RelicData data, Transform follow = null)
    {
        ApplyData(data);
        if (follow) { target = follow; }
        SetAlphaTarget(1f);
    }

    public void Hide()
    {
        SetAlphaTarget(0f);
        target = null;
    }

    public void ApplyData(RelicData data)
    {
        if (!data) return;
        if (iconImage) iconImage.sprite = data.icon;
        if (nameText) nameText.text = data.name;
        if (priceText) priceText.text = data.price.ToString() + priceSuffix;
        if (descriptionText) descriptionText.text = data.description; // �ʵ���� �ٸ��� �����ּ���
    }

    private void SetAlphaTarget(float a)
    {
        _targetAlpha = Mathf.Clamp01(a);
        if (!canvasGroup) return;
        // ���̵尡 0���� ������ �� ������ ����
        if (a > 0f && canvasGroup.alpha <= 0f && fadeDuration <= 0f)
            canvasGroup.alpha = a;
    }

    private void SetAlphaImmediate(float a)
    {
        _targetAlpha = a;
        if (canvasGroup)
        {
            canvasGroup.alpha = a;
            canvasGroup.interactable = canvasGroup.blocksRaycasts = a > 0.99f;
        }
    }
}
