using TMPro;
using UnityEngine;

public class TitleSetting : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI m_TextMeshPro;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;
    }

    private void Update()
    {
        m_TextMeshPro.text = GameStatus.Instance.playerAllMoney.ToString();
    }

}
