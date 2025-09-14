using UnityEngine;
using UnityEngine.Events;

public class DialogueService : MonoBehaviour
{
    public static DialogueService Instance { get; private set; }

    [Header("Runner Prefab (����+UI ����)")]
    [SerializeField] private DialogueRunner runnerPrefab; // ����+UI ���� ������ or ���� ����
    private DialogueRunner runner; // ���� ����ִ� ���� �ν��Ͻ�

    [Header("Events")]
    [Tooltip("���̾�α� ���� �� �ߵ� (�� ��ȯ �Ŀ��� ������)")]
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

        // ���� �̺�Ʈ ���� ���� �� �ٽ� ����
        runner.onDialogueEnd.RemoveListener(HandleRunnerEnd);
        runner.onDialogueEnd.AddListener(HandleRunnerEnd);

        runner.StartDialogue(data);
    }

    public bool IsPlaying => runner != null && runner.IsPlaying();

    private void EnsureRunner()
    {
        if (runner != null) return;
        runner = Instantiate(runnerPrefab, transform);
        // runner ���ο��� UI�� �ʿ� �� �����ϵ��� �ϰų�,
        // ���⼭ UI �������� runner.SetUI(Instantiate(uiPrefab))�� �����ص� ��
    }

    public void Stop()
    {
        if (runner != null && runner.IsPlaying())
            runner.EndDialogue();
    }

    private void HandleRunnerEnd()
    {
        Debug.Log("����");
        onDialogueEndGlobal?.Invoke();
    }
}
