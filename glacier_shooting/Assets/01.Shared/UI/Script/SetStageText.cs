using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetStageText : MonoBehaviour
{
    public TextMeshProUGUI stageText;


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (StageManager.Instance == null) return;
        int num = StageManager.Instance._stageIndex + 1;
        stageText.text = "STAGE : " + num;
    }

}
