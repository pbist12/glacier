// File: FPSCounter.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FPSCounter : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI tmpText;     // TMP 사용 시

    [Header("Sampling")]
    [Range(0.01f, 1f)] public float smooth = 0.1f;   // 지수이동평균 강도(클수록 반응 빠름)
    public bool showMs = true;

    float emaDt;     // 지수이동평균 Δt
    float minDt = float.MaxValue, maxDt = 0f; // 세션 동안 min/max
    int frame;

    void Awake()
    {
        emaDt = Time.unscaledDeltaTime;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        emaDt = Mathf.Lerp(emaDt, dt, smooth);
        frame++;

        minDt = Mathf.Min(minDt, dt);
        maxDt = Mathf.Max(maxDt, dt);

        if ((frame & 7) != 0) return; // 8프레임마다 갱신(깜빡임 방지)

        float fps = 1f / emaDt;
        float ms = emaDt * 1000f;

        string text = showMs
            ? $"{fps:0.0} FPS  ({ms:0.0} ms)\nmin {1000f * minDt:0.0} ms / max {1000f * maxDt:0.0} ms"
            : $"{fps:0.0} FPS";

        if (tmpText) tmpText.text = text;
    }
}
