using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 초 간단 선형 다이얼로그 진행기
/// - DialogueData(lines: DialogueLine[])를 순서대로 재생
/// - 타자 효과(선택): 스킵 시 즉시 완성
/// - 입력: New Input System 액션 또는 스페이스 키
/// - 이벤트: 시작/종료 콜백
/// 필요 컴포넌트: DialogueUI (name/body/panel 참조)
/// </summary>
public class DialogueRunner : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField, Tooltip("재생할 다이얼로그 데이터(SO)")]
    private DialogueData dialogue;

    [Header("UI 연결")]
    [SerializeField] private DialogueUI ui;

    [Header("진행 옵션")]
    [SerializeField, Tooltip("재생 시작 시 자동으로 시작")]
    private bool playOnStart = false;

    [SerializeField, Tooltip("타자 효과 사용 여부")]
    private bool useTypewriter = false;

    [SerializeField, Range(0.01f, 0.1f), Tooltip("타자 간격(초)")]
    private float typeInterval = 0.03f;

    [Header("입력 (New Input System)")]
    [SerializeField, Tooltip("다음 대사로 넘기는 액션(예: Submit/Interact)")]
    private InputActionReference nextAction;

    [Header("이벤트")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    // 내부 상태
    private int index = -1;
    private bool isPlaying = false;

    // 타자 상태
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
        // New Input System 사용 중에도 스페이스 백업을 쓰고 싶다면 주석 해제
        if (isPlaying && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Advance();

        // 타자 효과
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

    /// <summary> 다이얼로그 시작(바깥에서 호출 가능) </summary>
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
            ui.SetName("");      // 라인마다 덮어씀
            ui.SetBody("");
        }

        onDialogueStart?.Invoke();
        Advance(); // 첫 줄 표시
    }

    /// <summary> 다음 줄로 진행. 타자 중이면 즉시 완성 후 다음 입력에서 넘어감 </summary>
    public void Advance()
    {
        if (!isPlaying) return;

        // 타자 중이면 스킵(즉시 완성)
        if (useTypewriter && isTyping)
        {
            isTyping = false;
            if (ui != null) ui.SetBody(currentFullLine);
            return;
        }

        index++;

        // 끝났으면 종료
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

    /// <summary> 즉시 종료(패널 닫기, 상태 리셋) </summary>
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

    /// <summary> 현재 다이얼로그 재생 중인지 </summary>
    public bool IsPlaying() => isPlaying;

    /// <summary> 현재 라인 인덱스(0-base). 재생 전/종료 시 -1 </summary>
    public int CurrentIndex() => index;

    /// <summary> 재생 데이터를 런타임 중 교체(다음 StartDialogue에서 사용) </summary>
    public void SetDialogueData(DialogueData data) => dialogue = data;
}
