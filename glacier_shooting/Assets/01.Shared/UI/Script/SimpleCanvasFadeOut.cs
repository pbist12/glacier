using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class SimpleCanvasFadeOut : MonoBehaviour
{
    [Tooltip("알파 1 → 0 으로 줄어드는 시간(초)")]
    public float duration = 0.5f;
    public TextMeshProUGUI stageText;
    CanvasGroup cg;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 1f;                       // 시작값
        int num = StageManager.Instance._stageIndex + 1;
        stageText.text = "STAGE : " + num;

        StartCoroutine(Fade());
        cg.DOKill();                         // 중복 트윈 방지
    }

    void OnDisable() => cg.DOKill();         // 메모리/중복 방지

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(duration);
        cg.DOFade(0f, duration);

        yield return new WaitForSeconds(duration);
        if (StageManager.Instance._stageIndex == 0)
        {
            if (GameManager.Instance) GameManager.Instance.StartDialogue();
        }
        else
        {
            GameManager.Instance.StartNormalPhase();
        }
    }
}
