using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] SpriteRenderer cellSprite;
    public Vector2Int gridPosition;
    public int cellId;

    public bool ignoreStart = false;

    void Start()
    {
        GridController.Instance.SetGridElementOnStart(this);
    }

    public void InitCell(Vector2Int gridPos, Vector3 position, CellDataSO cellData, bool ignore = false)
    {
        gridPosition = gridPos;
        cellId = cellData.cellId;
        cellSprite.sprite = cellData.cellSprite;
        transform.position = position;
        ignoreStart = ignore;
        gameObject.SetActive(true);
    }

    public bool CheckMatch(int id)
    {
        return cellId == id;
    }
}
