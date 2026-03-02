using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCellScript : MonoBehaviour
{
    public gameManager game;

    public Vector2Int gridPos;
    public CellState nowState;
    // Start is called before the first frame update
    void Start()
    {
        game = gameManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        return;
        if (!game.grid.IsCellEmpty(game.grid.WorldToGridPos(transform.position)))
        {
            gridPos = game.grid.WorldToGridPos(transform.position);
            nowState = game.grid.debugCellState(gridPos);
        }
    }
}
