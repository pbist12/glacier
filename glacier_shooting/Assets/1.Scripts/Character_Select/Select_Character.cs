using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Select_Character : MonoBehaviour
{
    public CharacterData characterData;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset; // 인스펙터에서 씬 드래그
#endif
    [SerializeField] private string sceneName;      // 런타임에서 로드용 이름

#if UNITY_EDITOR
    void OnValidate()
    {
        if (sceneAsset != null)
            sceneName = sceneAsset.name; // 에디터에서 이름 동기화
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
