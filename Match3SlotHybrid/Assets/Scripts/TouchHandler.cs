using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : Singleton<TouchHandler>
{
    Cell firstCell;
    Cell secondCell;
    bool procesSwipe = false;
    bool processSpin = true;
    [Range(.05f, 1f)]
    public float swapTime = .5f;

    void Start()
    {
        Input.multiTouchEnabled = false;
    }

    #region TouchDetection
    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        GetTouchEditor();
#else
		GetTouchMobile();
#endif
    }

    void GetTouchEditor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckSpinButtonPress();
        }

        if (Input.GetMouseButton(0))
        {
            CheckCellInputs();
        }

        if (Input.GetMouseButtonUp(0) && procesSwipe)
        {
            ResetSelectedCells();
        }
    }

    void GetTouchMobile()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    CheckSpinButtonPress();
                    break;
                case TouchPhase.Moved:
                    CheckCellInputs();
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (procesSwipe)
                        ResetSelectedCells();
                    break;
            }
        }
    }

    void CheckSpinButtonPress()
    {
        if (!processSpin)
            return;

        Collider2D hit = Physics2D.OverlapPoint(GameManager.Instance.mainCam.ScreenToWorldPoint(Input.mousePosition));

        if (hit == null || !hit.TryGetComponent(out SpinView spinView))
            return;

        spinView.ExecSpinButton();
    }

    public void CheckCellInputs()
    {
        if (!procesSwipe)
            return;

        Collider2D hit = Physics2D.OverlapPoint(GameManager.Instance.mainCam.ScreenToWorldPoint(Input.mousePosition));

        if (hit == null || !hit.TryGetComponent(out Cell cell))
            return;

        if (firstCell == null)
            firstCell = cell;
        else if (firstCell != cell)
        {
            secondCell = cell;
            if (IsAbleToSwap())
            {
                SetProcessSwipe(false);
                SetProcessSpin(false);
                Swap();
            }
        }
    }

    IEnumerator CheckMatches()
    {
        List<Cell> firstMatchList = MatchFinder.Instance.FindMatchList(firstCell);
        List<Cell> secondMatchList = MatchFinder.Instance.FindMatchList(secondCell);
        if (firstMatchList == null && secondMatchList == null)
        {
            SetProcessSpin(true);
            SetProcessSwipe(true);
        }
        else
        {
            UIHandler.Instance.congratsPanel.Show();
        }

        ResetSelectedCells();

        yield return null;
    }

    bool IsAbleToSwap()
    {
        return GridController.Instance.GetCellNeighbours(firstCell).Contains(secondCell);
    }

    void Swap()
    {
        GridController.Instance.SwapCells(firstCell, secondCell);
        AnimateSwap().OnComplete(() =>
        {
            StartCoroutine(CheckMatches());
        });
    }

    Sequence AnimateSwap()
    {
        Vector3 temp = firstCell.transform.position;
        return DOTween.Sequence()
        .Join(firstCell.transform.DOMove(secondCell.transform.position, swapTime))
        .Join(secondCell.transform.DOMove(temp, swapTime));
    }

    public void SetProcessSwipe(bool value)
    {
        procesSwipe = value;
    }

    public void SetProcessSpin(bool value)
    {
        processSpin = value;
    }


    void ResetSelectedCells()
    {
        firstCell = null;
        secondCell = null;
    }

    #endregion
}
