using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Tooltip("�� Ʈ������ �Ʒ������� �ڵ� ���ε��� �����մϴ�. ����θ� �ڱ� �ڽ� ����.")]
    [SerializeField] private Transform root;

    [Tooltip("Awake���� �ڵ� ���ε� ����")]
    [SerializeField] private bool autoBindOnAwake = true;

    [Header("Texts")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerScore;
    public TextMeshProUGUI Text_Character;
    public TextMeshProUGUI Text_Gold;
    public TextMeshProUGUI Text_Bomb;
    public TextMeshProUGUI Text_Speed;
    public TextMeshProUGUI Text_MaxHealth;
    public TextMeshProUGUI Text_Damage;
    public TextMeshProUGUI Text_FireRate;
    public TextMeshProUGUI Text_Luck;


    [Header("Labels")]
    public string Label_PlayerHealth = "Player Health";
    public string Label_Character = "Character";
    public string Label_Gold = "Gold";
    public string Label_Bomb = "Bomb";
    public string Label_Speed = "Speed";
    public string Label_MaxHealth = "Max Health";
    public string Label_Damage = "Damage";
    public string Label_FireRate = "Fire Rate";
    public string Label_Luck = "Luck";

    void Awake()
    {
        if (root == null) root = transform;

        if (autoBindOnAwake)
            AutoBind();
    }

    // �����Ϳ��� �� �ٲ� �� �ڵ� ���ε� �õ�(�÷��� ������ ���� ���ϰ�)
    void OnValidate()
    {
        if (root == null) root = transform;
        AutoBind();
    }

    // ������ ��Ŭ�� �޴��� ��� ���� ����
    [ContextMenu("Auto Bind Now")]
    private void ContextAutoBind()
    {
        if (root == null) root = transform;
        AutoBind();
        Debug.Log("[PlayerUI] AutoBind complete.", this);
    }

    private void Update()
    {
        if (playerHealthText != null)
        {
            string hpCurrent = SafeInt(() => PlayerStatus.Instance.PlayerHealth);
            playerHealthText.text = $"Player Health : {hpCurrent}";
        }

        if (playerScore != null)
        {
            playerScore.text = $"Score : {GameManager.Instance.Score}";
        }
    }

    /// <summary>
    /// ���� ���� �ؽ�Ʈ�� �� ���� ���� (�ܺο��� �� ����)
    /// </summary>
    public void RefreshStats(float MoveSpeed, int MaxHealth, float AttackPower, float FireRate, int Luck)
    {
        if (Text_Character != null)
            Text_Character.text = $"{Label_Character} : {(PlayerStatus.Instance?.player != null ? PlayerStatus.Instance.player.playerName : "-")}";

        if (Text_Speed != null)
            Text_Speed.text = $"{Label_Speed} : {SafeFloat(() => MoveSpeed, "0.00")}";

        if (Text_MaxHealth != null)
            Text_MaxHealth.text = $"{Label_MaxHealth} : {SafeInt(() => MaxHealth)}";

        if (Text_Damage != null)
            Text_Damage.text = $"{Label_Damage} : {SafeFloat(() => AttackPower, "0.0")}";

        if (Text_FireRate != null)
            Text_FireRate.text = $"{Label_FireRate} : {SafeFloat(() => FireRate, "0.00")}";

        if (Text_Luck != null)
            Text_Luck.text = $"{Label_Luck} : {SafeInt(() => Luck)}";
    }

    /// <summary>
    /// ������ ��ġ ����(�ܺο��� �� ����)
    /// </summary>

    // ���ǻ� int ���� �����ε嵵 ����
    public void RefreshItem(int gold, int bomb)
    {
        if (Text_Gold != null)
            Text_Gold.text = $"{Label_Gold} : {SafeFloat(() => gold)}";

        if (Text_Bomb != null)
            Text_Bomb.text = $"{Label_Bomb} : {SafeFloat(() => bomb)}";
    }

    // ��������������������������������������������������������������������������������������������������������������������������
    // �ڵ� ���ε� ����
    private void AutoBind()
    {
        // TMP �ʵ��
        BindTMP(ref playerHealthText, nameof(playerHealthText));
        BindTMP(ref Text_Character, nameof(Text_Character));
        BindTMP(ref Text_Gold, nameof(Text_Gold));
        BindTMP(ref Text_Bomb, nameof(Text_Bomb));
        BindTMP(ref Text_Speed, nameof(Text_Speed));
        BindTMP(ref Text_MaxHealth, nameof(Text_MaxHealth));
        BindTMP(ref Text_Damage, nameof(Text_Damage));
        BindTMP(ref Text_FireRate, nameof(Text_FireRate));
        BindTMP(ref Text_Luck, nameof(Text_Luck));
    }

    private void BindTMP(ref TextMeshProUGUI field, string goName)
    {
        if (field != null) return;

        // 1) ���/�̸����� ���� ã��
        var t = root.Find(goName);
        if (t && t.TryGetComponent(out TextMeshProUGUI tmp))
        {
            field = tmp;
            return;
        }

        // 2) ���� ��ü ��ĵ�ؼ� �̸� ��ġ(��ҹ��� ����) ã��
        var all = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var x in all)
        {
            if (string.Equals(x.name, goName, StringComparison.OrdinalIgnoreCase))
            {
                field = x;
                return;
            }
        }
    }

    // ��������������������������������������������������������������������������������������������������������������������������
    // ���� ����(������Ƽ/�ʵ尡 ���ų� ���� ���� ���̾ ���� ���� "-")
    private string SafeInt(Func<int> getter)
    {
        try { return getter().ToString(); } catch { return "-"; }
    }
    private string SafeFloat(Func<float> getter, string format = null)
    {
        try
        {
            float v = getter();
            return string.IsNullOrEmpty(format) ? v.ToString() : v.ToString(format);
        }
        catch { return "-"; }
    }
}
