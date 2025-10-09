using TMPro;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    public GameObject root;
    public TextMeshProUGUI scoreLabel;

    public void Show(int score)
    {
        if (root) root.SetActive(true);
        if (scoreLabel) scoreLabel.text = $"Final Score : {score}";

        GameStatus.Instance.playerAllMoney += score / 100;
    }
}
