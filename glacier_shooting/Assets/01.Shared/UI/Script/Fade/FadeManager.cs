using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField, Min(0f)] private float defaultDuration = 0.5f;
    [SerializeField] private Ease ease = Ease.Linear;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool blockInputDuringFade = true;

    // runtime
    private Canvas _canvas;
    private Image _image;
    private Tween _tween;

    public bool IsFading { get; private set; }
    public float CurrentAlpha { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        blockInputDuringFade = false;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
        SetAlpha(0f);
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }

    private void BuildOverlay()
    {
        // Canvas
        var canvasGO = new GameObject("[FadeCanvas]");
        canvasGO.transform.SetParent(this.transform, false);   // 부모를 FadeManager로 설정
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = short.MaxValue;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Image
        var imageGO = new GameObject("[FadeImage]");
        imageGO.transform.SetParent(canvasGO.transform, false); // 부모를 Canvas로 설정
        _image = imageGO.AddComponent<Image>();

        var rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        _image.color = fadeColor;
        _image.raycastTarget = blockInputDuringFade;
    }

    private void SetAlpha(float a)
    {
        CurrentAlpha = Mathf.Clamp01(a);
        var c = _image.color;
        c.a = CurrentAlpha;
        _image.color = c;
    }

    private Tween PlayFade(float target, float duration, Color? colorOverride = null, Ease? easeOverride = null)
    {
        if (duration < 0f) duration = defaultDuration;
        if (colorOverride.HasValue)
        {
            var c = _image.color;
            c.r = colorOverride.Value.r;
            c.g = colorOverride.Value.g;
            c.b = colorOverride.Value.b;
            _image.color = c;
        }

        _tween?.Kill();
        IsFading = true;

        // 👉 페이드 시작 시 입력 차단 ON
        _image.raycastTarget = true;

        _tween = _image
            .DOFade(target, duration)
            .SetEase(easeOverride ?? ease)
            .SetUpdate(useUnscaledTime)
            .OnUpdate(() => CurrentAlpha = _image.color.a)
            .OnComplete(() =>
            {
                IsFading = false;
                // 👉 페이드 끝나면 입력 차단 해제
                _image.raycastTarget = false;
            });

        return _tween;
    }
    public Tween FadeOut(float duration = -1f, Color? colorOverride = null, Ease? easeOverride = null)
        => PlayFade(1f, duration, colorOverride, easeOverride);

    public Tween FadeIn(float duration = -1f, Color? colorOverride = null, Ease? easeOverride = null)
        => PlayFade(0f, duration, colorOverride, easeOverride);

    /// <summary>
    /// 어둡게 → 중간 작업 → 밝게 (코루틴 버전)
    /// </summary>
    public IEnumerator FadeOutThenIn(System.Func<IEnumerator> middle, float outDur = -1f, float inDur = -1f, Color? colorOverride = null, Ease? easeOverride = null)
    {
        yield return FadeOut(outDur, colorOverride, easeOverride).WaitForCompletion();
        if (middle != null) yield return middle();
        yield return FadeIn(inDur, colorOverride, easeOverride).WaitForCompletion();
    }
}
