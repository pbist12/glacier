using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("보스(EnemyHealth). 비워두면 씬에서 EnemyHealth.CurrentBoss를 사용합니다.")]
    [SerializeField] private EnemyHealth boss;

    [Tooltip("UI Slider 참조")]
    [SerializeField] private Slider slider;

    [Tooltip("Fill 이미지(선택). 할당하면 HP 비율에 따라 색을 바꿉니다.")]
    [SerializeField] private Image fillImage;

    [Header("Behavior")]
    [Tooltip("체력 표시가 부드럽게 변경되는 정도(지수 보간 강도)")]
    [Range(0.1f, 30f)] public float smooth = 10f;

    [Tooltip("보스가 죽으면 슬라이더를 숨깁니다.")]
    public bool hideWhenDead = true;

    [Header("Color By HP (선택)")]
    [Tooltip("HP 비율에 따라 색을 정하는 그라디언트 (0=빈피, 1=풀피)")]
    public Gradient hpGradient;

    private float _targetValue;

    private void Reset()
    {
        if (!slider) slider = GetComponentInChildren<Slider>();
        if (slider == null)
        {
            Debug.LogWarning("[BossUI] 자식에 Slider가 없습니다. Slider를 배치하거나 수동 할당하세요.");
        }
    }

    private void Awake()
    {
        if (slider == null)
        {
            Debug.LogError("[BossUI] 참조가 부족합니다. Slider를 확인하세요.");
            enabled = false;
            return;
        }

        slider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 명시적으로 바인딩 안 되어 있으면, 씬의 현재 보스 사용
        if (boss == null && EnemyHealth.CurrentBoss != null)
        {
            BindBossLike(EnemyHealth.CurrentBoss);
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    /// <summary>
    /// 통합 EnemyHealth용 보스 바인딩
    /// </summary>
    public void BindBossLike(EnemyHealth newBoss)
    {
        if (boss == newBoss) return;

        UnsubscribeEvents();
        boss = newBoss;

        if (boss == null)
        {
            // 보스가 없으면 UI 비활성
            if (slider != null)
            {
                slider.value = 0f;
                UpdateFillColorImmediate(0f);
                SetActiveIfNeeded(false);
            }
            return;
        }

        // 초기화 + 이벤트 구독
        InitializeFromBoss();
        SubscribeEvents();
    }

    public void UnbindBoss()
    {
        UnsubscribeEvents();
        boss = null;

        if (slider != null)
        {
            slider.value = 0f;
            UpdateFillColorImmediate(0f);
            SetActiveIfNeeded(false);
        }
    }

    private void InitializeFromBoss()
    {
        if (boss == null || slider == null) return;

        // 보스가 Boss/FinalBoss일 때만 표시하도록 안전장치(원한다면 제거 가능)
        if (!boss.IsBossLike)
        {
            SetActiveIfNeeded(false);
            return;
        }

        slider.gameObject.SetActive(true);

        // Slider 기본 설정
        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1f, boss.maxHP);
        float clamped = Mathf.Clamp(boss.HP, slider.minValue, slider.maxValue);
        slider.value = clamped;
        _targetValue = clamped;

        UpdateFillColorImmediate(Ratio());
        SetActiveIfNeeded(true);
    }

    private void SubscribeEvents()
    {
        if (boss == null) return;
        boss.onHpChanged += OnBossHpChanged;
        boss.onDeath += OnBossDeath;
    }

    private void UnsubscribeEvents()
    {
        if (boss == null) return;
        boss.onHpChanged -= OnBossHpChanged;
        boss.onDeath -= OnBossDeath;
    }

    private void OnBossHpChanged(float hp, float max)
    {
        if (slider == null) return;

        // maxHP가 동적으로 변할 수 있으므로 항상 보정
        slider.maxValue = Mathf.Max(1f, max);
        _targetValue = Mathf.Clamp(hp, 0f, slider.maxValue);

        // 색상은 즉시 반영(시각적 반응성)
        UpdateFillColorImmediate(max > 0f ? Mathf.Clamp01(hp / max) : 0f);

        // 죽음은 onDeath에서 처리
        if (hp <= 0f)
        {
            slider.value = 0f;
            if (hideWhenDead) SetActiveIfNeeded(false);
        }
        else
        {
            SetActiveIfNeeded(true);
        }
    }

    private void OnBossDeath()
    {
        if (slider == null) return;

        slider.value = 0f;
        UpdateFillColorImmediate(0f);
        if (hideWhenDead) SetActiveIfNeeded(false);
    }

    private void Update()
    {
        if (boss == null || slider == null || !boss.IsBossLike) return;

        // 지수 보간으로 부드럽게 이동
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        slider.value = Mathf.Lerp(slider.value, _targetValue, t);
        // 색상은 이벤트에서 이미 즉시 반영되지만,
        // 프레임별로도 안전하게 동기화하고 싶다면 아래 한 줄 유지
        UpdateFillColorImmediate(Ratio());
    }

    private float Ratio()
    {
        if (boss == null || boss.maxHP <= 0f) return 0f;
        return Mathf.Clamp01(boss.HP / boss.maxHP);
    }

    private void UpdateFillColorImmediate(float ratio)
    {
        if (fillImage == null || hpGradient == null) return;
        fillImage.color = hpGradient.Evaluate(ratio);
    }

    private void SetActiveIfNeeded(bool active)
    {
        if (slider != null && slider.gameObject.activeSelf != active)
            slider.gameObject.SetActive(active);
    }
}
