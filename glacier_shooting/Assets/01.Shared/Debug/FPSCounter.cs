// File: FPSCounter.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FPSCounter : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI tmpText;     // TMP ��� ��

    [Header("Sampling")]
    [Range(0.01f, 1f)] public float smooth = 0.1f;   // �����̵���� ����(Ŭ���� ���� ����)
    public bool showMs = true;

    float emaDt;     // �����̵���� ��t
    float minDt = float.MaxValue, maxDt = 0f; // ���� ���� min/max
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

        if ((frame & 7) != 0) return; // 8�����Ӹ��� ����(������ ����)

        float fps = 1f / emaDt;
        float ms = emaDt * 1000f;

        string text = showMs
            ? $"{fps:0.0} FPS  ({ms:0.0} ms)\nmin {1000f * minDt:0.0} ms / max {1000f * maxDt:0.0} ms"
            : $"{fps:0.0} FPS";

        if (tmpText) tmpText.text = text;
    }
}
