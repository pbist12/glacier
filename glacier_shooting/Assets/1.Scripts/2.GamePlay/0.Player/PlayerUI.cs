using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Tooltip("이 트랜스폼 아래에서만 자동 바인딩을 수행합니다. 비워두면 자기 자신 기준.")]
    [SerializeField] private Transform root;

    [Tooltip("Awake에서 자동 바인딩 실행")]
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

    // 에디터에서 값 바뀔 때 자동 바인딩 시도(플레이 전에도 연결 편하게)
    void OnValidate()
    {
        if (root == null) root = transform;
        AutoBind();
    }

    // 에디터 우클릭 메뉴로 즉시 실행 가능
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
    /// 스탯 관련 텍스트를 한 번에 갱신 (외부에서 값 전달)
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
    /// 아이템 수치 갱신(외부에서 값 전달)
    /// </summary>

    // 편의상 int 버전 오버로드도 제공
    public void RefreshItem(int gold, int bomb)
    {
        if (Text_Gold != null)
            Text_Gold.text = $"{Label_Gold} : {SafeFloat(() => gold)}";

        if (Text_Bomb != null)
            Text_Bomb.text = $"{Label_Bomb} : {SafeFloat(() => bomb)}";
    }

    // ─────────────────────────────────────────────────────────────
    // 자동 바인딩 구현
    private void AutoBind()
    {
        // TMP 필드들
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

        // 1) 경로/이름으로 직접 찾기
        var t = root.Find(goName);
        if (t && t.TryGetComponent(out TextMeshProUGUI tmp))
        {
            field = tmp;
            return;
        }

        // 2) 하위 전체 스캔해서 이름 일치(대소문자 무시) 찾기
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

    // ─────────────────────────────────────────────────────────────
    // 안전 래퍼(프로퍼티/필드가 없거나 아직 구현 전이어도 에러 없이 "-")
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
