using System.Collections.Generic;
using UnityEngine;

public class GridController : Singleton<GridController>
{
    public CellPooling cellPooling;
    public SpinController spinController;
    [SerializeField] SpriteRenderer gridBg;
    [SerializeField] Transform gridMask;
    [SerializeField] SpinView spinView;

    public Vector2Int gridSize;
    public Cell[,] grid;

    [Range(0f, .5f)]
    public float cellSpacing = .25f;
    public CellDataSO[] cells;

    public bool ignoreRandomGrid = false;
    [HideInInspector] public float distanceBetweenCenters;

    void Start()
    {
        CreateGrid();
    }

    #region Grid
    public void CreateGrid()
    {
        gridSize = GetGridSize();
        ClearGrid();

        float cellUnitSize = cellPooling.cellPrefab.transform.localScale.x;
        distanceBetweenCenters = cellUnitSize + cellSpacing;
        float distanceBetweenCells = distanceBetweenCenters * .5f;
        float backgroundSpacing = .225f;
        Vector2 bgSize = new Vector2(gridSize.x * distanceBetweenCenters + backgroundSpacing, gridSize.y * distanceBetweenCenters + backgroundSpacing);
        InitGridBackground(bgSize);
        FitCameraToGrid(distanceBetweenCenters);

        float xStart = -gridSize.x * distanceBetweenCells + distanceBetweenCells;
        float yStart = -gridSize.y * distanceBetweenCells + distanceBetweenCells;
        Vector2 startPosition = new Vector2(xStart, yStart);

        List<CellDataSO> predefinedCells = GetAvailableCells();
        Dictionary<Vector2Int, int> cellIds = new ();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                List<int> exceptList = new ();
                Vector2Int gridPos = new (x, y);
                if (cellIds.TryGetValue(gridPos + Vector2Int.down, out int vPrevFirst) && cellIds.TryGetValue(gridPos + Vector2Int.down * 2, out int vPrevSec))
                {
                    if (vPrevFirst == vPrevSec)
                        exceptList.Add(vPrevFirst);
                }

                if (cellIds.TryGetValue(gridPos + Vector2Int.left, out int hPrevFirst) && cellIds.TryGetValue(gridPos + Vector2Int.left * 2, out int hPrevSec))
                {
                    if (hPrevFirst == hPrevSec)
                        exceptList.Add(hPrevFirst);
                }

                CellDataSO cellData = GetCellDataExceptList(predefinedCells, exceptList);
                Cell cell = cellPooling.SpawnCell();
                cell.InitCell(gridPos, startPosition, cellData);
                startPosition.y += distanceBetweenCenters;

                cellIds.Add(gridPos, cellData.cellId);
            }

            startPosition.y = yStart;
            startPosition.x += distanceBetweenCenters;
        }

        SpawnExtraCells();
    }

    public CellDataSO GetRandomCellData()
    {
        int randomIndex = Random.Range(0, cells.Length);
        return cells[randomIndex];
    }

    public CellDataSO GetCellDataExceptList(List<CellDataSO> predefinedCells, List<int> exceptList, bool ignorePredefined = false)
    {
        List<CellDataSO> tempDataSet = new ((predefinedCells.Count > 0 && !ignorePredefined) ? predefinedCells : cells);
        for (int i = 0; i < exceptList.Count; i++)
        {
            tempDataSet.RemoveAll(cell => cell.cellId == exceptList[i]);
        }

        int tempDataSetCount = tempDataSet.Count;
        if (tempDataSetCount == 0)
            return GetCellDataExceptList(predefinedCells, exceptList, true);

        int randomIndex = Random.Range(0, tempDataSet.Count);
        CellDataSO cellData = tempDataSet[randomIndex];
        predefinedCells.Remove(cellData);
        return cellData;
    }

    public List<CellDataSO> GetAvailableCells()
    {
        List<CellDataSO> tempCells = new ();
        for (int i = 0; i < cells.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                tempCells.Add(cells[i]);
            }
        }

        return tempCells;
    }

    void ClearGrid()
    {
        grid = null;
        cellPooling.ClearCells();
        Transform cellParent = cellPooling.cellParent;
        if (cellParent == null)
            return;

        int cellsCount = cellParent.childCount;
        for (int i = 0; i < cellsCount; i++)
        {
            DestroyImmediate(cellParent.GetChild(0).gameObject);
        }
    }

    public void SetGridElementOnStart(Cell cell)
    {
        if (grid == null)
            grid = new Cell[gridSize.x, gridSize.y];

        grid[cell.gridPosition.x, cell.gridPosition.y] = cell;
    }

    public void SetGridElement(Cell cell)
    {
        grid[cell.gridPosition.x, cell.gridPosition.y] = cell;
    }

    void SpawnExtraCells()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            Cell extraCell = cellPooling.SpawnCell();
            extraCell.gameObject.SetActive(false);
        }
    }

    void InitGridBackground(Vector2 size)
    {
        gridBg.size = size;
        gridMask.localScale = size - Vector2.one * .125f;

        spinView.transform.position = (-size.y * .5f - spinView.spinButtonOffset) * Vector3.up;
    }

    Vector2Int GetGridSize()
    {
        if (ignoreRandomGrid)
        {
            Vector2Int tempGridSize = gridSize;
            tempGridSize.x = Mathf.Max(3, gridSize.x);
            tempGridSize.y = Mathf.Max(3, gridSize.y);
            return tempGridSize;
        }

        Vector2Int[] gridSizes = { new (5, 5), new(7, 7) };
        return gridSizes[Random.Range(0, gridSizes.Length)];
    }

    #endregion

    #region Neighbour
    public List<Cell> GetCellNeighbours(Cell cell)
    {
        List<Cell> neighbours = new ();
        Vector2Int gridPos = cell.gridPosition;

        if (gridPos.x - 1 >= 0)
            neighbours.Add(grid[gridPos.x - 1, gridPos.y]);
        if (gridPos.x + 1 < gridSize.x)
            neighbours.Add(grid[gridPos.x + 1, gridPos.y]);
        if (gridPos.y - 1 >= 0)
            neighbours.Add(grid[gridPos.x, gridPos.y - 1]);
        if (gridPos.y + 1 < gridSize.y)
            neighbours.Add(grid[gridPos.x, gridPos.y + 1]);

        return neighbours;
    }

    public Cell GetNeighbourByDirection(Cell cell, Direction dir)
    {
        Cell neighbour = null;
        Vector2Int gridPos = cell.gridPosition;

        switch (dir)
        {
            case Direction.left:
                if (gridPos.x - 1 >= 0)
                    neighbour = grid[gridPos.x - 1, gridPos.y];
                break;
            case Direction.right:
                if (gridPos.x + 1 < gridSize.x)
                    neighbour = grid[gridPos.x + 1, gridPos.y];
                break;
            case Direction.up:
                if (gridPos.y + 1 < gridSize.y)
                    neighbour = grid[gridPos.x, gridPos.y + 1];
                break;
            case Direction.down:
                if (gridPos.y - 1 >= 0)
                    neighbour = grid[gridPos.x, gridPos.y - 1];
                break;
        }
        return neighbour;
    }

    #endregion

    #region CellSwap
    public void SwapCells(Cell first, Cell second)
    {
        Vector2Int temp = first.gridPosition;
        first.gridPosition = second.gridPosition;
        second.gridPosition = temp;
        SetGridElement(first);
        SetGridElement(second);
    }
    #endregion

    void FitCameraToGrid(float cellDistance)
    {
        float horizontalDistance = gridSize.x * cellDistance;
        float verticalDistance = gridSize.y * cellDistance;
        float horizontalOffset = horizontalDistance * .05f;
        float verticalOffset = horizontalDistance * .2f;

        int sizeDiff = gridSize.y - gridSize.x;
        float orthographicSize = sizeDiff > 3 ? verticalDistance + verticalOffset - 4f : horizontalDistance + horizontalOffset + 1.5f;
        GameManager gameManager = FindObjectOfType<GameManager>();

        float refAspect = 1125f / 2436f;
        float targetAspect = gameManager.uiRect.rect.width / gameManager.uiRect.rect.height;
        float aspectDiff = refAspect - targetAspect;
        float aspectMultiplier = 10f;

        orthographicSize += aspectDiff * aspectMultiplier;
        gameManager.mainCam.orthographicSize = Mathf.Max(8, orthographicSize);
    }
}
