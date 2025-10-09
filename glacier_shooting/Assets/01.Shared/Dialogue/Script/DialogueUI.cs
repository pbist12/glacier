using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private GameObject panel;           // ���̾�α� �ڽ� ��Ʈ
    [SerializeField] private TMP_Text nameText;          // ȭ�� �̸�
    [SerializeField] private TMP_Text bodyText;          // ����

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
