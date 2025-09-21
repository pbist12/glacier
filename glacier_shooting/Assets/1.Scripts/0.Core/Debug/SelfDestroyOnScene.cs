using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SelfDestroyOnScene : MonoBehaviour
{
    [Header("여기에 '죽을' 씬 이름을 넣으세요 (예: Title, MainMenu)")]
    public string[] targetScenes;

    [Tooltip("true: targetScenes에 '포함되면' 파괴 / false: targetScenes에 '없으면' 파괴(=화이트리스트)")]
    public bool destroyOnMatch = true;

    [Tooltip("정확히 같은 이름 대신 부분 포함(Contains)으로 매칭할지")]
    public bool useContainsMatch = false;

    [Tooltip("이 객체를 여기서 DDOL로 등록할지(이미 등록돼 있다면 끄세요)")]
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
        // Additive 로드로 activeScene이 안 바뀌는 경우도 커버
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
            // 즉시 영향 최소화를 위해 먼저 꺼두고 파괴 예약
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
