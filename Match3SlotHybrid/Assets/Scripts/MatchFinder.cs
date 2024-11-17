using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : Singleton<MatchFinder>
{
    public List<Cell> FindMatchList(Cell cell)
    {
        Vector2Int gridSize = GridController.Instance.gridSize;
        bool[,] visitedCells = new bool[gridSize.x, gridSize.y];
        List<Cell> resultCells = new ();
        List<List<Cell>> directionalMatches = new () { new (), new () };
        FindMatches(cell, cell, Direction.right, directionalMatches, visitedCells);

        foreach (List<Cell> matchList in directionalMatches)
        {
            if (matchList.Count > 1)
                resultCells.AddRange(matchList);
        }
        resultCells.Add(cell);

        return resultCells.Count > 2 ? resultCells : null;
    }

    void FindMatches(Cell newCell, Cell selectedCell, Direction dir, List<List<Cell>> directionalMatches, bool[,] visitedCells)
    {
        if (newCell == null)
            return;

        Vector2Int gridPos = newCell.gridPosition;

        if (visitedCells[gridPos.x, gridPos.y])
            return;

        visitedCells[gridPos.x, gridPos.y] = true;

        if (newCell.CheckMatch(selectedCell.cellId))
        {
            if (newCell == selectedCell)
            {
                for (int i = 0; i < 4; i++)
                {
                    dir = (Direction)i;
                    Cell neighbour = GridController.Instance.GetNeighbourByDirection(newCell, dir);
                    if (neighbour != null)
                        FindMatches(neighbour, selectedCell, dir, directionalMatches, visitedCells);
                }
            }
            else
            {
                int listIndex = (dir == Direction.left || dir == Direction.right) ? 0 : 1;
                directionalMatches[listIndex].Add(newCell);

                Cell neighbour = GridController.Instance.GetNeighbourByDirection(newCell, dir);
                if (neighbour != null)
                    FindMatches(neighbour, selectedCell, dir, directionalMatches, visitedCells);
            }
        }
    }
}
