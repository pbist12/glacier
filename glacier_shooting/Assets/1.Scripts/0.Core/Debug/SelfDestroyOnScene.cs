using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SelfDestroyOnScene : MonoBehaviour
{
    [Header("���⿡ '����' �� �̸��� �������� (��: Title, MainMenu)")]
    public string[] targetScenes;

    [Tooltip("true: targetScenes�� '���ԵǸ�' �ı� / false: targetScenes�� '������' �ı�(=ȭ��Ʈ����Ʈ)")]
    public bool destroyOnMatch = true;

    [Tooltip("��Ȯ�� ���� �̸� ��� �κ� ����(Contains)���� ��Ī����")]
    public bool useContainsMatch = false;

    [Tooltip("�� ��ü�� ���⼭ DDOL�� �������(�̹� ��ϵ� �ִٸ� ������)")]
    public bool registerAsDontDestroyOnLoad = false;

    void Awake()
    {
        if (registerAsDontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Evaluate(newScene);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive �ε�� activeScene�� �� �ٲ�� ��쵵 Ŀ��
        Evaluate(scene);
    }

    private void Evaluate(Scene scene)
    {
        if (targetScenes == null || targetScenes.Length == 0) return;

        string name = scene.name;
        bool match = false;

        foreach (var t in targetScenes)
        {
            if (string.IsNullOrEmpty(t)) continue;
            if (useContainsMatch ? name.Contains(t) : name == t)
            {
                match = true;
                break;
            }
        }

        bool shouldDestroy = destroyOnMatch ? match : !match;
        if (shouldDestroy)
        {
            // ��� ���� �ּ�ȭ�� ���� ���� ���ΰ� �ı� ����
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
