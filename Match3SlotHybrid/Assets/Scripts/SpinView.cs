using DG.Tweening;
using UnityEngine;

public class SpinView : MonoBehaviour
{
    [SerializeField] Transform button;
    [SerializeField] SpriteRenderer spinTextRenderer;
    [SerializeField] Sprite spinSprite;
    [SerializeField] Sprite stopSprite;

    public float spinButtonOffset = 1.75f;
    public float defaultYPos = .3f;
    public float pressedYPos = .1f;

    bool pressed;

    public void ExecSpinButton()
    {
        pressed = !pressed;
        MoveButton();
        if (pressed)
            GridController.Instance.spinController.SpinCells();
        else
            GridController.Instance.spinController.StopCells();
    }

    void MoveButton()
    {
        float targetYPos = pressed ? pressedYPos : defaultYPos;
        Ease ease = pressed ? Ease.OutBack : Ease.InBack;
        button.DOLocalMoveY(targetYPos, .15f).SetEase(ease, 2.5f).OnComplete(OnMoveComplete);
    }

    void OnMoveComplete()
    {
        spinTextRenderer.sprite = pressed ? stopSprite : spinSprite;
    }
}
