using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "SimpleDialogue/Data")]
public class DialogueData : ScriptableObject
{
    [Header("��� ��� (������� ����)")]
    public DialogueLine[] lines;
}


[System.Serializable]
public class DialogueLine
{
    [Tooltip("�� ����� ȭ�� �̸�")]
    public string speaker;

    [TextArea(2, 5)]
    [Tooltip("��� ����")]
    public string text;
}