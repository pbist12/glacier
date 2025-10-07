using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class SimpleCanvasFadeOut : MonoBehaviour
{
    [Tooltip("���� 1 �� 0 ���� �پ��� �ð�(��)")]
    public float duration = 0.5f;
    public TextMeshProUGUI stageText;
    CanvasGroup cg;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 1f;                       // ���۰�
        int num = StageManager.Instance._stageIndex + 1;
        stageText.text = "STAGE : " + num;

        StartCoroutine(Fade());
        cg.DOKill();                         // �ߺ� Ʈ�� ����
    }

    void OnDisable() => cg.DOKill();         // �޸�/�ߺ� ����

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
