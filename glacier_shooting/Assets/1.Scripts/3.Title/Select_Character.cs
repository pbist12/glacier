using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Select_Character : MonoBehaviour
{
    public CharacterData characterData;
    public TextMeshProUGUI CharacterName;
    
    public TextMeshProUGUI info1;
    public TextMeshProUGUI info2;
    public TextMeshProUGUI info3;

    [SerializeField] private string sceneName;      // ��Ÿ�ӿ��� �ε�� �̸�

    private void Start()
    {
        CharacterName.text = characterData.playerName;
    }

    public void SelectCharacter()
    {
        GameStatus.Instance.characterData = characterData;

        info1.text = "Player Health : " + characterData.maxLife;
        info2.text = "Player Speed : " + characterData.moveSpeed;
        info3.text = "Player FireRate : " + characterData.fireRate;
    }
}
