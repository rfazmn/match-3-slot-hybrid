using DG.Tweening;
using TMPro;
using UnityEngine;

public class CongratsPanel : Panel
{
    [SerializeField] CanvasGroup panelCG;
    [SerializeField] Transform panel;
    [SerializeField] TMP_Text[] perfectTexts;
    [SerializeField] Transform continueButton;

    [SerializeField] float animTime = .15f;
    [SerializeField] float delayBetweenCharacters = .03f;

    public override void Show()
    {
        ResetPanel();

        DOTween.Sequence()
            .Join(panelCG.DOFade(1f, animTime).SetEase(Ease.Linear))
            .Append(panel.DOScale(1f, animTime).SetEase(Ease.OutBack))
            .Append(AnimateCharacters())
            .Append(continueButton.DOScale(1f, animTime).SetEase(Ease.OutBack))
            .OnComplete(() => panelCG.blocksRaycasts = true);
    }

    public override void Hide()
    {
        panelCG.SetActiveness(false);
    }

    public override void ResetPanel()
    {
        panel.localScale = Vector3.zero;
        continueButton.localScale = Vector3.zero;
        for (int i = 0; i < perfectTexts.Length; i++)
        {
            perfectTexts[i].transform.localScale = Vector3.one * 3f;
            perfectTexts[i].alpha = 0f;
        }
    }

    Sequence AnimateCharacters()
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < perfectTexts.Length; i++)
        {
            int index = i;
            perfectTexts[index].color = Random.ColorHSV(0f, .6f, 1f, 1f, 1f, 1f, 0f, 0f);
            seq.Join(perfectTexts[index].DOFade(1f, 0f).SetDelay(delayBetweenCharacters));
            seq.Join(perfectTexts[index].transform.DOScale(1f, .1f).SetEase(Ease.Linear).SetDelay(delayBetweenCharacters));
        }

        return seq;
    }

}
