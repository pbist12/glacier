using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneTitle : MonoBehaviour
{
    public string sceneName;

    public void Load()
    {
        if (GameStatus.Instance.characterData == null) return;
        SceneLoader.Instance.LoadScene(sceneName);
    }

}
