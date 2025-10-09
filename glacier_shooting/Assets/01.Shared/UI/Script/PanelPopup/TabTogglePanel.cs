using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TabTogglePanel : MonoBehaviour
{
    [Header("UI Panel Settings")]
    public RectTransform panel;          // ������ �г�
    public float moveDistance = 300f;    // ���������� �̵��� �Ÿ�
    public float duration = 0.3f;        // �̵� �ִϸ��̼� �ð�
    public Ease easeType = Ease.OutBack; // �˵��� ������ ���� Ease

    private bool isOpen = false;         // ���� ���� �ִ��� ����
    private Vector2 originalPos;         // �г� ���� ��ġ

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
            // �ݱ�: ���� �ڸ��� �̵�
            panel.DOAnchorPos(originalPos, duration).SetEase(easeType);
        }
        else
        {
            // ����: ���������� x��ŭ �̵�
            Vector2 targetPos = originalPos + new Vector2(moveDistance, 0);
            panel.DOAnchorPos(targetPos, duration).SetEase(easeType);
        }

        isOpen = !isOpen;
    }
}
