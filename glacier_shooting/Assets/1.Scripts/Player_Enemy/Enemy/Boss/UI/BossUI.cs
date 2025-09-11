using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("���� ��Ʈ�ѷ�. ����θ� �ڵ����� ������ ù ��° BossController�� ã���ϴ�.")]
    [SerializeField] private BossHealth boss;

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
        slider = GetComponentInChildren<Slider>();
        if (slider == null)
        {
            Debug.LogWarning("[BossHealthSlider] �ڽĿ� Slider�� �����ϴ�. Slider�� ��ġ�ϰų� ���� �Ҵ��ϼ���.");
        }
    }

    private void Awake()
    {
        if (slider == null)
        {
            Debug.LogError("[BossHealthSlider] ������ �����մϴ�. Boss/Slider�� Ȯ���ϼ���.");
            enabled = false;
            return;
        }

        slider.gameObject.SetActive(false);
    }

    /// <summary>
    /// �ܺο��� �������� ������ ���ε��� �� ���
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

        // Slider �⺻ ����
        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1f, boss.maxHP);
        slider.value = Mathf.Clamp(boss.hp, slider.minValue, slider.maxValue);
        _targetValue = slider.value;

        // �ʱ� ���� �ݿ�
        UpdateFillColorImmediate();
        SetActiveIfNeeded(true);
    }

    private void Update()
    {
        if (boss == null || slider == null) return;

        // ���� ��� ó��
        if (boss.hp <= 0f)
        {
            slider.value = 0f;
            UpdateFillColorImmediate();

            if (hideWhenDead)
                SetActiveIfNeeded(false);

            return;
        }

        // ��ǥ�� = ���� HP
        _targetValue = Mathf.Clamp(boss.hp, 0f, boss.maxHP);

        // ���� �������� �ε巴��
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        slider.value = Mathf.Lerp(slider.value, _targetValue, t);

        // ���� ������Ʈ
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
