using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("보스 컨트롤러. 비워두면 자동으로 씬에서 첫 번째 BossController를 찾습니다.")]
    [SerializeField] private BossHealth boss;

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
        slider = GetComponentInChildren<Slider>();
        if (slider == null)
        {
            Debug.LogWarning("[BossHealthSlider] 자식에 Slider가 없습니다. Slider를 배치하거나 수동 할당하세요.");
        }
    }

    private void Awake()
    {
        if (slider == null)
        {
            Debug.LogError("[BossHealthSlider] 참조가 부족합니다. Boss/Slider를 확인하세요.");
            enabled = false;
            return;
        }

        slider.gameObject.SetActive(false);
    }

    /// <summary>
    /// 외부에서 동적으로 보스를 바인딩할 때 사용
    /// </summary>
    public void BindBoss(BossHealth newBoss)
    {
        boss = newBoss;
        InitializeFromBoss();
    }

    private void InitializeFromBoss()
    {
        if (boss == null || slider == null) return;

        slider.gameObject.SetActive(true);

        // Slider 기본 설정
        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1f, boss.maxHP);
        slider.value = Mathf.Clamp(boss.hp, slider.minValue, slider.maxValue);
        _targetValue = slider.value;

        // 초기 색상 반영
        UpdateFillColorImmediate();
        SetActiveIfNeeded(true);
    }

    private void Update()
    {
        if (boss == null || slider == null) return;

        // 보스 사망 처리
        if (boss.hp <= 0f)
        {
            slider.value = 0f;
            UpdateFillColorImmediate();

            if (hideWhenDead)
                SetActiveIfNeeded(false);

            return;
        }

        // 목표값 = 현재 HP
        _targetValue = Mathf.Clamp(boss.hp, 0f, boss.maxHP);

        // 지수 보간으로 부드럽게
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        slider.value = Mathf.Lerp(slider.value, _targetValue, t);

        // 색상 업데이트
        UpdateFillColorImmediate();
    }

    private void UpdateFillColorImmediate()
    {
        if (fillImage == null || hpGradient == null) return;

        float ratio = 0f;
        if (boss != null && boss.maxHP > 0f)
            ratio = Mathf.Clamp01(boss.hp / boss.maxHP);

        fillImage.color = hpGradient.Evaluate(ratio);
    }

    private void SetActiveIfNeeded(bool active)
    {
        if (slider != null && slider.gameObject.activeSelf != active)
            slider.gameObject.SetActive(active);
    }
}
