using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Select_Character : MonoBehaviour
{
    public CharacterData characterData;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset; // �ν����Ϳ��� �� �巡��
#endif
    [SerializeField] private string sceneName;      // ��Ÿ�ӿ��� �ε�� �̸�

#if UNITY_EDITOR
    void OnValidate()
    {
        if (sceneAsset != null)
            sceneName = sceneAsset.name; // �����Ϳ��� �̸� ����ȭ
    }
#endif
    public void Load()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    public void SelectCharacter()
    {
        PlayerData.Instance.characterData = characterData;
        Load();
    }
}
