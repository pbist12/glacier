// File: ShopItemInfoUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemInfoUI : MonoBehaviour
{
    [Header("Refs (자동 바인딩 지원)")]
    public CanvasGroup canvasGroup;
    public GameObject canvas;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI descriptionText;

    [Header("Follow (월드 스페이스에서 슬롯을 따라다님)")]
    public bool followTarget = true;
    public Transform target;                // 따라갈 대상(보통 ShopSlot.transform)
    public Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Behavior")]
    [Min(0f)] public float fadeDuration = 0.12f;
    public string priceSuffix = " G";

    private float _alphaVel;
    private float _targetAlpha = 0f;

    #region Singleton (선택)
    public static ShopItemInfoUI Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 안전 바인딩
        if (!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        // 사용자가 올려준 경로 명칭에 맞춰 자동 찾기 (없으면 인스펙터 수동 연결)
        if (!iconImage)
            iconImage = transform.Find("Image/Image_Item")?.GetComponent<Image>();
        if (!nameText)
            nameText = transform.Find("Image/Text_Item_Name")?.GetComponent<TextMeshProUGUI>();
        if (!priceText)
            priceText = transform.Find("Image/Text_Item_Price")?.GetComponent<TextMeshProUGUI>();
        if (!descriptionText)
            descriptionText = transform.Find("Image/Text_Item_Desc")?.GetComponent<TextMeshProUGUI>();

        // 초기 비가시화
        SetAlphaImmediate(0f);
    }
    #endregion

    void LateUpdate()
    {
        // 부드러운 페이드
        if (canvasGroup)
        {
            // SmoothDamp 대신 MoveTowards로 간단히
            if (!Mathf.Approximately(canvasGroup.alpha, _targetAlpha))
            {
                float step = (fadeDuration > 0f)
                    ? Time.deltaTime / fadeDuration
                    : 1f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, step);
                canvasGroup.interactable = canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.99f;
            }
        }

        // 타겟 따라가기
        if (followTarget && target)
        {
            transform.position = target.position + worldOffset;
            // 필요하면 카메라 빌보드:
            if (Camera.main) transform.forward = Camera.main.transform.forward;
        }
    }

    public void ShowFor(RelicData data, Transform follow = null)
    {
        canvas.gameObject.SetActive(true);
        ApplyData(data);
        if (follow) { target = follow; }
        SetAlphaTarget(1f);
    }

    public void Hide()
    {
        SetAlphaTarget(0f);
        target = null;
        canvas.gameObject.SetActive(false);
    }

    public void ApplyData(RelicData data)
    {
        if (!data) return;
        if (iconImage) iconImage.sprite = data.icon;
        if (nameText) nameText.text = data.name;
        if (priceText) priceText.text = data.price.ToString() + priceSuffix;
        if (descriptionText) descriptionText.text = data.description; // 필드명이 다르면 맞춰주세요
    }

    private void SetAlphaTarget(float a)
    {
        _targetAlpha = Mathf.Clamp01(a);
        if (!canvasGroup) return;
        // 페이드가 0에서 시작할 때 깜빡임 방지
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
