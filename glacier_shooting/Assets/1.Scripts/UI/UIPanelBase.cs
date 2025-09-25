using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public abstract class UIPanelBase : MonoBehaviour
{
    [Header("First Select (Optional)")]
    [SerializeField] private Selectable firstSelected;

    [Header("Anim")]
    [SerializeField] private CanvasGroup cg;
    [SerializeField] private float fadeTime = 0.15f;

    protected virtual void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        cg.DOKill();
        cg.DOFade(1f, fadeTime);
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // 패드 대응: 첫 포커스 지정
        if (firstSelected)
            EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);
    }

    public virtual void Hide()
    {
        cg.DOKill();
        cg.DOFade(0f, fadeTime).OnComplete(() =>
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
            gameObject.SetActive(false);
        });
    }
}
