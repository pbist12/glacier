using UnityEngine;
using UnityEngine.Events;

public class DialogueService : MonoBehaviour
{
    public static DialogueService Instance { get; private set; }

    [Header("Runner Prefab (로직+UI 포함)")]
    [SerializeField] private DialogueRunner runnerPrefab; // 로직+UI 포함 프리팹 or 로직 전용
    private DialogueRunner runner; // 현재 살아있는 러너 인스턴스

    [Header("Events")]
    [Tooltip("다이얼로그 종료 시 발동 (씬 전환 후에도 유지됨)")]
    public UnityEvent onDialogueEndGlobal;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(DialogueData data)
    {
        EnsureRunner();

        // 기존 이벤트 구독 해제 후 다시 연결
        runner.onDialogueEnd.RemoveListener(HandleRunnerEnd);
        runner.onDialogueEnd.AddListener(HandleRunnerEnd);

        runner.StartDialogue(data);
    }

    public bool IsPlaying => runner != null && runner.IsPlaying();

    private void EnsureRunner()
    {
        if (runner != null) return;
        runner = Instantiate(runnerPrefab, transform);
        // runner 내부에서 UI를 필요 시 생성하도록 하거나,
        // 여기서 UI 프리팹을 runner.SetUI(Instantiate(uiPrefab))로 주입해도 됨
    }

    public void Stop()
    {
        if (runner != null && runner.IsPlaying())
            runner.EndDialogue();
    }

    private void HandleRunnerEnd()
    {
        Debug.Log("종료");
        onDialogueEndGlobal?.Invoke();
    }
}
