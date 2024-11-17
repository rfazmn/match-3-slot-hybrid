using UnityEngine;

[CreateAssetMenu(fileName = "New Cell Data", menuName = "Cell/New Cell")]
public class CellDataSO : ScriptableObject
{
    public int cellId;
    public Sprite cellSprite;
}
