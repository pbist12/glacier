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
    }

    public void Play(DialogueData data)
    {
        EnsureRunner();

        // ���� �̺�Ʈ ���� ���� �� �ٽ� ����
        runner.onDialogueEnd.RemoveListener(HandleRunnerEnd);
        runner.onDialogueEnd.AddListener(HandleRunnerEnd);

        GameManager.Instance.Paused = true;
        runner.StartDialogue(data);
    }

    public bool IsPlaying => runner != null && runner.IsPlaying();

    private void EnsureRunner()
    {
        if (runner != null) return;
        runner = Instantiate(runnerPrefab, transform);
    }

    public void Stop()
    {
        if (runner != null && runner.IsPlaying())
        {
            runner.EndDialogue();
            GameManager.Instance.Paused = false;
        }
    }

    private void HandleRunnerEnd()
    {
        Debug.Log("����");
        onDialogueEndGlobal?.Invoke();
    }
}
