using UnityEngine;
using UnityEngine.UI;

public class TitleCharacterImage : MonoBehaviour
{
    public Image mainImage;

    void Start()
    {
        // ������ �� ���� ó��
        if (mainImage != null)
            mainImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void Update()
    {
        if (GameStatus.Instance.characterData != null)
        {
            // ���õ� �� ��������Ʈ �Ҵ� + ������(���)
            mainImage.sprite = GameStatus.Instance.characterData.portrait;
            mainImage.color = Color.white;
        }
        else
        {
            // ���� �ȵ� �� ����
            mainImage.sprite = null;
            mainImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
