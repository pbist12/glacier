using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("����(EnemyHealth). ����θ� ������ EnemyHealth.CurrentBoss�� ����մϴ�.")]
    [SerializeField] private EnemyHealth boss;

    [Tooltip("UI Slider ����")]
    [SerializeField] private Slider slider;

    [Tooltip("Fill �̹���(����). �Ҵ��ϸ� HP ������ ���� ���� �ٲߴϴ�.")]
    [SerializeField] private Image fillImage;

    [Header("Behavior")]
    [Tooltip("ü�� ǥ�ð� �ε巴�� ����Ǵ� ����(���� ���� ����)")]
    [Range(0.1f, 30f)] public float smooth = 10f;

    [Tooltip("������ ������ �����̴��� ����ϴ�.")]
    public bool hideWhenDead = true;

    [Header("Color By HP (����)")]
    [Tooltip("HP ������ ���� ���� ���ϴ� �׶���Ʈ (0=����, 1=Ǯ��)")]
    public Gradient hpGradient;

    private float _targetValue;

    private void Reset()
    {
        if (!slider) slider = GetComponentInChildren<Slider>();
        if (slider == null)
        {
            Debug.LogWarning("[BossUI] �ڽĿ� Slider�� �����ϴ�. Slider�� ��ġ�ϰų� ���� �Ҵ��ϼ���.");
        }
    }

    private void Awake()
    {
        if (slider == null)
        {
            Debug.LogError("[BossUI] ������ �����մϴ�. Slider�� Ȯ���ϼ���.");
            enabled = false;
            return;
        }

        slider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // ��������� ���ε� �� �Ǿ� ������, ���� ���� ���� ���
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
    /// ���� EnemyHealth�� ���� ���ε�
    /// </summary>
    public void BindBossLike(EnemyHealth newBoss)
    {
        if (boss == newBoss) return;

        UnsubscribeEvents();
        boss = newBoss;

        if (boss == null)
        {
            // ������ ������ UI ��Ȱ��
            if (slider != null)
            {
                slider.value = 0f;
                UpdateFillColorImmediate(0f);
                SetActiveIfNeeded(false);
            }
            return;
        }

        // �ʱ�ȭ + �̺�Ʈ ����
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

        // ������ Boss/FinalBoss�� ���� ǥ���ϵ��� ������ġ(���Ѵٸ� ���� ����)
        if (!boss.IsBossLike)
        {
            SetActiveIfNeeded(false);
            return;
        }

        slider.gameObject.SetActive(true);

        // Slider �⺻ ����
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

        // maxHP�� �������� ���� �� �����Ƿ� �׻� ����
        slider.maxValue = Mathf.Max(1f, max);
        _targetValue = Mathf.Clamp(hp, 0f, slider.maxValue);

        // ������ ��� �ݿ�(�ð��� ������)
        UpdateFillColorImmediate(max > 0f ? Mathf.Clamp01(hp / max) : 0f);

        // ������ onDeath���� ó��
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

        // ���� �������� �ε巴�� �̵�
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        slider.value = Mathf.Lerp(slider.value, _targetValue, t);
        // ������ �̺�Ʈ���� �̹� ��� �ݿ�������,
        // �����Ӻ��ε� �����ϰ� ����ȭ�ϰ� �ʹٸ� �Ʒ� �� �� ����
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
