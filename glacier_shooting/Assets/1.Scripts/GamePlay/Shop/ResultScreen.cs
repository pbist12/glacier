using TMPro;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    public GameObject root;
    public TextMeshProUGUI scoreLabel;

    public void Show(int score)
    {
        if (scoreLabel) scoreLabel.text = $"Final Score : {score}";
        if (root) root.SetActive(true);
    }
}
