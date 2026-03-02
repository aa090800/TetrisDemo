using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public GridManager grid;
    public CellCtrl cell;
    public UICtrl ui;
    public PieceSpriteCtrl spriteCtrl;
    private void Awake()
    {
        instance = this;
    }

}
