using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinController : MonoBehaviour
{
    public float startSpeed = 2f;
    public float maxSpeed = 20f;
    public float minSpeed = 1f;
    public float speedupDuration = 1f;
    public float columnSpeedupDelay = .1f;
    public float columnReleaseDuration = 1.5f;
    public int numOfCellsToSlowdown = 5;
    float yMin;
    float yMax;

    List<Queue<CellDataSO>> predefinedBoard = new();
    public List<float> columnSpeeds = new();
    bool[] stoppedColumns;

    Vector2Int gridSize = new(-1, -1);

    void InitializeValues()
    {
        if (gridSize.x == -1)
        {
            gridSize = GridController.Instance.gridSize;
            yMin = GridController.Instance.grid[0, 0].transform.position.y - GridController.Instance.distanceBetweenCenters;
            yMax = GridController.Instance.grid[0, gridSize.y - 1].transform.position.y + GridController.Instance.distanceBetweenCenters;
        }
    }

    public void SpinCells()
    {
        InitializeValues();
        TouchHandler.Instance.SetProcessSpin(false);
        TouchHandler.Instance.SetProcessSwipe(false);
        predefinedBoard.Clear();
        columnSpeeds.Clear();
        stoppedColumns = new bool[gridSize.x];
        float delay = 0f;
        for (int x = 0; x < gridSize.x; x++)
        {
            columnSpeeds.Add(startSpeed);
            StartCoroutine(IncreaseColumnSpeed(x, delay));
            Vector3 tempPos = GridController.Instance.grid[x, 0].transform.position;
            tempPos.y = yMax;
            Cell extraCell = GridController.Instance.cellPooling.GetAvailableCell();

            extraCell.InitCell(new Vector2Int(x, 0), tempPos, GridController.Instance.GetRandomCellData(), true);
            MoveCell(extraCell, new Vector2Int(x, gridSize.y - 1), delay, Ease.InBack);

            for (int y = 0; y < gridSize.y; y++)
            {
                Cell cell = GridController.Instance.grid[x, y];
                MoveCell(cell, new Vector2Int(x, y), delay, Ease.InBack);
            }

            delay += columnSpeedupDelay;
        }

        StartCoroutine(DelayedCall(speedupDuration + delay, () => TouchHandler.Instance.SetProcessSpin(true)));
    }

    void MoveCell(Cell cell, Vector2Int gridPos, float delay, Ease ease)
    {
        Vector3 nextPos = cell.transform.position;

        if (nextPos.y <= yMin)
        {
            nextPos.y = yMax;
            gridPos.y = gridSize.y;

            cell.InitCell(gridPos, nextPos, GridController.Instance.GetRandomCellData());
        }

        gridPos.y -= 1;
        nextPos.y -= GridController.Instance.distanceBetweenCenters;
        cell.transform.DOMove(nextPos, columnSpeeds[gridPos.x]).SetSpeedBased(true).SetEase(ease).SetDelay(delay).SetTarget(cell).OnComplete(() => OnMoveComplete(cell, gridPos, Ease.Linear));
    }

    void OnMoveComplete(Cell cell, Vector2Int gridPos, Ease ease)
    {
        cell.gridPosition = gridPos;
        if (stoppedColumns[gridPos.x])
            return;

        MoveCell(cell, cell.gridPosition, 0f, ease);
    }

    IEnumerator IncreaseColumnSpeed(int columnIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        float startValue = columnSpeeds[columnIndex];
        float elapsedTime = 0f;
        while (elapsedTime < speedupDuration)
        {
            elapsedTime += Time.deltaTime;
            columnSpeeds[columnIndex] = Mathf.Lerp(startValue, maxSpeed, elapsedTime / speedupDuration);
            yield return null;
        }
    }

    void DecreaseColumnSpeed(int columnIndex)
    {
        float nextValue = columnSpeeds[columnIndex] - GetSlowdownValue();
        columnSpeeds[columnIndex] = Mathf.Max(minSpeed, nextValue);
    }

    float GetSlowdownValue()
    {
        return maxSpeed / numOfCellsToSlowdown;
    }

    float GetColumnReleaseDelay()
    {
        return columnReleaseDuration / gridSize.x;
    }

    public void StopCells()
    {
        TouchHandler.Instance.SetProcessSpin(false);
        FillPredefinedBoard();
        StartCoroutine(StopCellsCo());
    }

    IEnumerator StopCellsCo()
    {
        CellPooling cellPooling = GridController.Instance.cellPooling;
        cellPooling.cellList.Sort((a, b) => a.gridPosition.x.CompareTo(b.gridPosition.x));
        float columnReleaseDelay = GetColumnReleaseDelay();
        for (int i = 0; i < gridSize.x; i++)
        {
            int columnCellsCount = gridSize.y + 1;
            List<Cell> cellList = cellPooling.cellList.GetRange(i * columnCellsCount, columnCellsCount);

            stoppedColumns[i] = true;
            while (DOTween.IsTweening(cellList[0]))
                yield return null;

            StartCoroutine(StopColumn(cellList, columnCellsCount));
            yield return new WaitForSeconds(columnReleaseDelay);
        }
    }

    IEnumerator StopColumn(List<Cell> columnCells, int columnCellsCount)
    {
        int gridPosX = columnCells[0].gridPosition.x;
        int gridYCounter = 0;
        DecreaseColumnSpeed(gridPosX);
        for (int i = 0; i < gridSize.y; i++)
        {
            if (i + numOfCellsToSlowdown > gridSize.y)
                DecreaseColumnSpeed(gridPosX);

            float animDuration = 1f / columnSpeeds[gridPosX];
            Ease ease = (i == gridSize.y - 1) ? Ease.OutBack : Ease.Linear;

            for (int j = 0; j < columnCellsCount; j++)
            {
                Cell cell = columnCells[j];
                Vector3 nextPos = cell.transform.position;

                if (nextPos.y <= yMin)
                {
                    nextPos.y = yMax;
                    Vector2Int gridPos = cell.gridPosition;
                    gridPos.y = gridYCounter;
                    CellDataSO data = GetPredefinedDataWithIndex(gridPosX);
                    cell.InitCell(gridPos, nextPos, data);
                    gridYCounter++;
                }

                nextPos.y -= GridController.Instance.distanceBetweenCenters;
                cell.transform.DOMove(nextPos, animDuration).SetEase(ease);
            }

            yield return new WaitForSeconds(animDuration);
        }

        CheckCellPositionsOnStop(columnCells);
        CheckCellsStopped(gridPosX);
    }

    void CheckCellsStopped(int gridPosX)
    {
        if (gridPosX != gridSize.x - 1)
            return;

        TouchHandler.Instance.SetProcessSpin(true);
        TouchHandler.Instance.SetProcessSwipe(true);
    }

    void CheckCellPositionsOnStop(List<Cell> column)
    {
        for (int i = 0; i < column.Count; i++)
        {
            Cell cell = column[i];
            if (cell.transform.position.y <= yMin)
                cell.gameObject.SetActive(false);
            else
                GridController.Instance.SetGridElement(cell);
        }
    }

    void FillPredefinedBoard()
    {
        List<CellDataSO> predefinedCells = GridController.Instance.GetAvailableCells();
        Dictionary<Vector2Int, int> board = new();

        for (int x = 0; x < gridSize.x; x++)
        {
            Queue<CellDataSO> column = new();
            for (int y = 0; y < gridSize.y; y++)
            {
                List<int> exceptList = new();
                Vector2Int gridPos = new(x, y);
                if (board.TryGetValue(gridPos + Vector2Int.down, out int vPrevFirst) && board.TryGetValue(gridPos + Vector2Int.down * 2, out int vPrevSec))
                {
                    if (vPrevFirst == vPrevSec)
                        exceptList.Add(vPrevFirst);
                }

                if (board.TryGetValue(gridPos + Vector2Int.left, out int hPrevFirst) && board.TryGetValue(gridPos + Vector2Int.left * 2, out int hPrevSec))
                {
                    if (hPrevFirst == hPrevSec)
                        exceptList.Add(hPrevFirst);
                }

                CellDataSO cellData = GridController.Instance.GetCellDataExceptList(predefinedCells, exceptList);
                board.Add(gridPos, cellData.cellId);
                column.Enqueue(cellData);
            }

            predefinedBoard.Add(column);
        }
    }

    CellDataSO GetPredefinedDataWithIndex(int columnIndex)
    {
        return predefinedBoard[columnIndex].Dequeue();
    }

    #region Util
    IEnumerator DelayedCall(float delay, Action func)
    {
        yield return new WaitForSeconds(delay);
        func();
    }
    #endregion
}
