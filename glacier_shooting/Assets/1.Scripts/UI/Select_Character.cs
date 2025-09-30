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

    [SerializeField] private float lerpSpeed = 5f;  // �� ��ȯ �ӵ�
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 1f, 1f); // ��ο� ����
    [SerializeField] private Color unselectedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // ��ο� ����

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

        // ���� ���� ���� ����
        isSelected = GameStatus.Instance.characterData == characterData;
        targetColor = isSelected ? selectedColor : unselectedColor;

        // �� ������ Lerp ���� �� �ڿ������� �� ��ȯ
        characterImage.color = Color.Lerp(characterImage.color, targetColor, Time.deltaTime * lerpSpeed);
    }

    public void SelectCharacter()
    {
        // ���� �� ������ ����
        GameStatus.Instance.characterData = characterData;

        // ���õ� ĳ������ ���� UI ����
        info1.text = "Player Health : " + characterData.maxLife;
        info2.text = "Player Speed : " + characterData.moveSpeed;
        info3.text = "Player FireRate : " + characterData.fireRate;
    }
}
