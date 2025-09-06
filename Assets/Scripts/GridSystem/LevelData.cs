using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Kayip Gezegen/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid Settings")]
    public int width = 9;
    public int height = 16;
    public Vector2 cellSize = Vector2.one; // 1x1 grid için

    [Header("Level Points")]
    public Vector2Int playerStart = new Vector2Int(4, 1);
    public Vector2Int exitPortal = new Vector2Int(4, 14);

    [Header("Level Elements")]
    public List<Cell> cells = new List<Cell>();

    public enum TileType
    {
        Empty,
        Wall,
        Block,
        Enemy,
        Door,
        Sensor,
        CrystalRed,
        CrystalBlue,
        CrystalYellow,
        CrystalPurple,
        Trap
    }

    [System.Serializable]
    public struct Cell
    {
        public Vector2Int pos;
        public TileType type;

        public Cell(int x, int y, TileType tileType)
        {
            pos = new Vector2Int(x, y);
            type = tileType;
        }
    }

    // Editor'de kolay ekleme için helper metodlar
    public void AddCell(int x, int y, TileType type)
    {
        cells.Add(new Cell(x, y, type));
    }

    public void ClearCells()
    {
        cells.Clear();
    }
}