using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField, Min(0f)] private float defaultFade = 0.5f;
    [SerializeField] private bool waitAt90ThenActivate = true;
    [SerializeField] private Ease fadeEase = Ease.Linear;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (FadeManager.Instance == null)
            new GameObject("FadeManager").AddComponent<FadeManager>();
    }

    public void LoadScene(string sceneName, float fadeDuration = -1f)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] sceneName is null or empty.");
            return;
        }
        if (fadeDuration < 0f) fadeDuration = defaultFade;
        StartCoroutine(CoLoad(sceneName, fadeDuration));
    }

    public void ReloadCurrent(float fadeDuration = -1f)
    {
        var current = SceneManager.GetActiveScene().name;
        LoadScene(current, fadeDuration);
    }

    private IEnumerator CoLoad(string sceneName, float fade)
    {
        // 1) 페이드 아웃 완료 대기(DOTween)
        yield return FadeManager.Instance.FadeOut(fade, easeOverride: fadeEase).WaitForCompletion();

        // 2) 비동기 로드
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = !waitAt90ThenActivate;

        if (waitAt90ThenActivate)
        {
            while (op.progress < 0.9f) yield return null;
            op.allowSceneActivation = true;
        }

        while (!op.isDone) yield return null;

        // 3) 페이드 인
        yield return FadeManager.Instance.FadeIn(fade, easeOverride: fadeEase).WaitForCompletion();
    }
}
