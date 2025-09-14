using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// �� ���� ���� ���̾�α� �����
/// - DialogueData(lines: DialogueLine[])�� ������� ���
/// - Ÿ�� ȿ��(����): ��ŵ �� ��� �ϼ�
/// - �Է�: New Input System �׼� �Ǵ� �����̽� Ű
/// - �̺�Ʈ: ����/���� �ݹ�
/// �ʿ� ������Ʈ: DialogueUI (name/body/panel ����)
/// </summary>
public class DialogueRunner : MonoBehaviour
{
    [Header("������")]
    [SerializeField, Tooltip("����� ���̾�α� ������(SO)")]
    private DialogueData dialogue;

    [Header("UI ����")]
    [SerializeField] private DialogueUI ui;

    [Header("���� �ɼ�")]
    [SerializeField, Tooltip("��� ���� �� �ڵ����� ����")]
    private bool playOnStart = false;

    [SerializeField, Tooltip("Ÿ�� ȿ�� ��� ����")]
    private bool useTypewriter = false;

    [SerializeField, Range(0.01f, 0.1f), Tooltip("Ÿ�� ����(��)")]
    private float typeInterval = 0.03f;

    [Header("�Է� (New Input System)")]
    [SerializeField, Tooltip("���� ���� �ѱ�� �׼�(��: Submit/Interact)")]
    private InputActionReference nextAction;

    [Header("�̺�Ʈ")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    // ���� ����
    private int index = -1;
    private bool isPlaying = false;

    // Ÿ�� ����
    private bool isTyping = false;
    private string currentFullLine = "";
    private float typeTimer = 0f;
    private int typedCount = 0;

    // -------- Unity LifeCycle --------
    private void Awake()
    {
        if (ui != null) ui.ShowPanel(false);

        if (nextAction != null)
        {
            nextAction.action.performed += OnNextPerformed;
            nextAction.action.Enable();
        }

        if (playOnStart && dialogue != null)
            StartDialogue(dialogue);
    }

    private void OnDestroy()
    {
        if (nextAction != null)
        {
            nextAction.action.performed -= OnNextPerformed;
            nextAction.action.Disable();
        }
    }

    private void Update()
    {
        // New Input System ��� �߿��� �����̽� ����� ���� �ʹٸ� �ּ� ����
        if (isPlaying && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Advance();

        // Ÿ�� ȿ��
        if (isPlaying && useTypewriter && isTyping)
        {
            typeTimer -= Time.unscaledDeltaTime;
            if (typeTimer <= 0f)
            {
                typeTimer = typeInterval;
                typedCount = Mathf.Min(typedCount + 1, currentFullLine.Length);
                if (ui != null) ui.SetBody(currentFullLine.Substring(0, typedCount));
                if (typedCount >= currentFullLine.Length)
                    isTyping = false;
            }
        }
    }

    private void OnNextPerformed(InputAction.CallbackContext _)
    {
        if (isPlaying) Advance();
    }

    // -------- Public API --------

    /// <summary> ���̾�α� ����(�ٱ����� ȣ�� ����) </summary>
    public void StartDialogue(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
            return;

        dialogue = data;
        index = -1;
        isPlaying = true;

        if (ui != null)
        {
            ui.ShowPanel(true);
            ui.SetName("");      // ���θ��� ���
            ui.SetBody("");
        }

        onDialogueStart?.Invoke();
        Advance(); // ù �� ǥ��
    }

    /// <summary> ���� �ٷ� ����. Ÿ�� ���̸� ��� �ϼ� �� ���� �Է¿��� �Ѿ </summary>
    public void Advance()
    {
        if (!isPlaying) return;

        // Ÿ�� ���̸� ��ŵ(��� �ϼ�)
        if (useTypewriter && isTyping)
        {
            isTyping = false;
            if (ui != null) ui.SetBody(currentFullLine);
            return;
        }

        index++;

        // �������� ����
        if (dialogue == null || dialogue.lines == null || index >= dialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogue.lines[index];

        if (ui != null)
        {
            ui.SetName(line.speaker ?? "");
        }

        if (!useTypewriter)
        {
            if (ui != null) ui.SetBody(line.text);
        }
        else
        {
            currentFullLine = line.text ?? "";
            typedCount = 0;
            typeTimer = 0f;
            isTyping = true;
            if (ui != null) ui.SetBody("");
        }
    }

    /// <summary> ��� ����(�г� �ݱ�, ���� ����) </summary>
    public void EndDialogue()
    {
        isPlaying = false;
        isTyping = false;
        currentFullLine = "";
        typedCount = 0;

        if (ui != null)
        {
            ui.SetBody("");
            ui.ShowPanel(false);
        }

        onDialogueEnd?.Invoke();
    }

    /// <summary> ���� ���̾�α� ��� ������ </summary>
    public bool IsPlaying() => isPlaying;

    /// <summary> ���� ���� �ε���(0-base). ��� ��/���� �� -1 </summary>
    public int CurrentIndex() => index;

    /// <summary> ��� �����͸� ��Ÿ�� �� ��ü(���� StartDialogue���� ���) </summary>
    public void SetDialogueData(DialogueData data) => dialogue = data;
}
