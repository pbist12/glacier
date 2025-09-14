using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "SimpleDialogue/Data")]
public class DialogueData : ScriptableObject
{
    [Header("대사 목록 (순서대로 진행)")]
    public DialogueLine[] lines;
}


[System.Serializable]
public class DialogueLine
{
    [Tooltip("이 대사의 화자 이름")]
    public string speaker;

    [TextArea(2, 5)]
    [Tooltip("대사 내용")]
    public string text;
}