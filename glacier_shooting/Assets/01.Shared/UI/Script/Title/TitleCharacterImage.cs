using UnityEngine;
using UnityEngine.UI;

public class TitleCharacterImage : MonoBehaviour
{
    public Image mainImage;

    void Start()
    {
        // 시작할 때 투명 처리
        if (mainImage != null)
            mainImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void Update()
    {
        if (GameStatus.Instance.characterData != null)
        {
            // 선택됨 → 스프라이트 할당 + 불투명(흰색)
            mainImage.sprite = GameStatus.Instance.characterData.portrait;
            mainImage.color = Color.white;
        }
        else
        {
            // 선택 안됨 → 투명
            mainImage.sprite = null;
            mainImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
