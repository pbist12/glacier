using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject panel;           // 다이얼로그 박스 루트
    [SerializeField] private TMP_Text nameText;          // 화자 이름
    [SerializeField] private TMP_Text bodyText;          // 본문

    public void ShowPanel(bool show)
    {
        if (panel != null) panel.SetActive(show);
    }

    public void SetName(string speaker)
    {
        if (nameText != null) nameText.text = string.IsNullOrEmpty(speaker) ? "" : speaker;
    }

    public void SetBody(string text)
    {
        if (bodyText != null) bodyText.text = text;
    }
}
