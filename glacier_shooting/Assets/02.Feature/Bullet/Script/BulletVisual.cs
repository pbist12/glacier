using UnityEngine;

[DisallowMultipleComponent]
public class BulletVisual : MonoBehaviour
{
    [Header("Awake에서 기본값 자동 캡처")]
    [SerializeField] private bool captureOnAwake = true;

    private SpriteRenderer sr;
    private TrailRenderer tr;
    private ParticleSystem ps;

    // 기본 값 캐시
    private Color baseColor = Color.white;
    private Vector3 baseScale;
    private Gradient trailBaseGradient;
    private ParticleSystem.MinMaxGradient psBaseColor;

    private bool captured;

    void Awake()
    {
        if (captureOnAwake) CacheDefaults();
    }

    /// <summary>현재 프리팹(실행중 인스턴스)의 비주얼 상태를 '기본값'으로 저장</summary>
    public void CacheDefaults()
    {
        sr ??= GetComponentInChildren<SpriteRenderer>();
        tr ??= GetComponentInChildren<TrailRenderer>();
        ps ??= GetComponentInChildren<ParticleSystem>();

        if (sr) baseColor = sr.color;
        if (tr) trailBaseGradient = tr.colorGradient;
        if (ps) { var m = ps.main; psBaseColor = m.startColor; }

        baseScale = transform.localScale;
        captured = true;
    }

    /// <summary>풀에서 꺼낼 때 항상 호출: 기본 색/스케일/트레일/파티클로 리셋</summary>
    public void ResetVisuals()
    {
        if (!captured) CacheDefaults();

        if (sr) sr.color = baseColor;
        if (tr) tr.colorGradient = trailBaseGradient;

        if (ps)
        {
            var m = ps.main;
            m.startColor = psBaseColor;
        }

        transform.localScale = baseScale;
    }

    /// <summary>색 틴트 적용 (Trail/Particle도 함께 맞춰줌)</summary>
    public void ApplyTint(Color c)
    {
        if (sr) sr.color = c;

        if (tr)
        {
            // 기존 알파는 유지하고, 색만 통일해서 틴트
            var g = new Gradient();
            var alphaKeys = trailBaseGradient.alphaKeys.Length > 0
                ? trailBaseGradient.alphaKeys
                : new GradientAlphaKey[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) };

            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(c, 0f),
                new GradientColorKey(c, 1f)
            };
            g.SetKeys(colorKeys, alphaKeys);
            tr.colorGradient = g;
        }

        if (ps)
        {
            var m = ps.main;
            var start = m.startColor;
            start.color = c;   // MinMaxGradient 단일 색으로 세팅
            m.startColor = start;
        }
    }

    /// <summary>스케일 배율 적용 (누적 X, 기본 스케일 기준)</summary>
    public void ApplySizeMul(float mul)
    {
        transform.localScale = baseScale * mul;
    }

    /// <summary>ShotRequest와 사이즈 배율을 입력받아 한 번에 적용</summary>
    public void ApplyFromRequest(ShotRequest req, float finalSizeMul)
    {
        ResetVisuals();

        if (req.tint.HasValue)
            ApplyTint(req.tint.Value);

        if (Mathf.Abs(finalSizeMul - 1f) > 0.0001f)
            ApplySizeMul(finalSizeMul);
    }
}
