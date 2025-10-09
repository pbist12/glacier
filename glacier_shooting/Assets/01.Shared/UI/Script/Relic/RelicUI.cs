using UnityEngine;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour
{
    public Image relicImage;

    public void UpdateRelicImage(Sprite relic)
    {
        relicImage.sprite = relic;
    }
}
