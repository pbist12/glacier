using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Select_Character : MonoBehaviour
{
    public CharacterData characterData;
    public TextMeshProUGUI CharacterName;

    public TextMeshProUGUI info1;
    public TextMeshProUGUI info2;
    public TextMeshProUGUI info3;

    public Image characterImage;

    [SerializeField] private float lerpSpeed = 5f;  // 색 전환 속도
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 1f, 1f); // 어두운 상태
    [SerializeField] private Color unselectedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // 어두운 상태

    [SerializeField] private Color targetColor;
    [SerializeField] private bool isSelected;

    private void Start()
    {
        characterImage.color = unselectedColor;
        CharacterName.text = characterData.playerName;
    }

    private void Update()
    {
        if (GameStatus.Instance == null) return;

        // 현재 선택 여부 판정
        isSelected = GameStatus.Instance.characterData == characterData;
        targetColor = isSelected ? selectedColor : unselectedColor;

        // 매 프레임 Lerp 적용 → 자연스럽게 색 전환
        characterImage.color = Color.Lerp(characterImage.color, targetColor, Time.deltaTime * lerpSpeed);
    }

    public void SelectCharacter()
    {
        // 선택 시 데이터 갱신
        GameStatus.Instance.characterData = characterData;

        // 선택된 캐릭터의 정보 UI 갱신
        info1.text = "Player Health : " + characterData.maxLife;
        info2.text = "Player Speed : " + characterData.moveSpeed;
        info3.text = "Player FireRate : " + characterData.fireRate;
    }
}
