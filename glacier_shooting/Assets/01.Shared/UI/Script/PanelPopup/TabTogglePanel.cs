using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TabTogglePanel : MonoBehaviour
{
    [Header("UI Panel Settings")]
    public RectTransform panel;          // 움직일 패널
    public float moveDistance = 300f;    // 오른쪽으로 이동할 거리
    public float duration = 0.3f;        // 이동 애니메이션 시간
    public Ease easeType = Ease.OutBack; // 쫀득한 느낌을 위한 Ease

    private bool isOpen = false;         // 현재 열려 있는지 여부
    private Vector2 originalPos;         // 패널 원래 위치

    void Start()
    {
        if (panel == null)
            panel = GetComponent<RectTransform>();

        originalPos = panel.anchoredPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePanel();
        }
    }

    void TogglePanel()
    {
        if (isOpen)
        {
            // 닫기: 원래 자리로 이동
            panel.DOAnchorPos(originalPos, duration).SetEase(easeType);
        }
        else
        {
            // 열기: 오른쪽으로 x만큼 이동
            Vector2 targetPos = originalPos + new Vector2(moveDistance, 0);
            panel.DOAnchorPos(targetPos, duration).SetEase(easeType);
        }

        isOpen = !isOpen;
    }
}
