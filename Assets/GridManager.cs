using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellState { Empty,Filled}

public class GridManager : MonoBehaviour
{
    [SerializeField] int width = 10;
    [SerializeField] int height = 24;

    public int Width => width;
    public int Height => height;

    float cellSize = 0.4f;

    private CellState[,] cells;

    Vector2 StartPos;

    private void Awake()
    {
        InitializeGrid();
    }
    private void Start()
    {
        
    }
    void InitializeGrid()
    {
        cells = new CellState[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = CellState.Empty;
            }
        }

        float Posx = -width * cellSize / 2;
        float Posy = -20 * cellSize / 2;
        StartPos = new Vector3(Posx, Posy);
    }
    //出界檢測
    public bool IsInside(Vector2Int pos)
    {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) return true;
        else return false;
    }
    //檢測是否為Empty
    public bool IsCellEmpty(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;
        if (cells[pos.x, pos.y] == CellState.Empty) return true;
        else return false;
    }

    public Vector2Int WorldToGridPos(Vector3 pos)
    {
        int x = Mathf.RoundToInt((pos.x - StartPos.x) / cellSize - 0.5f);
        int y = Mathf.RoundToInt((pos.y - StartPos.y )/ cellSize - 0.5f);
        return new Vector2Int(x,y);
    }
    public Vector3 GridToWorldPos(Vector2Int pos)
    {
        float x = StartPos.x + pos.x * cellSize + cellSize * 0.5f;
        float y = StartPos.y + pos.y * cellSize + cellSize * 0.5f;
        return new Vector3(x, y);
    }

    public void SetPieceFilled(Vector2Int pos)
    {
        cells[pos.x, pos.y] = CellState.Filled;
    }

    public void SetPieceEmpty(Vector2Int pos)
    {
        cells[pos.x, pos.y] = CellState.Empty;
    }
     
    //Debug用
    public CellState debugCellState(Vector2Int pos)
    {
        return cells[pos.x, pos.y];
    }
}
