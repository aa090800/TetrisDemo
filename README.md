# Tetries-俄羅斯方塊
經典遊戲「俄羅斯方塊」。

## 遊戲玩法
**移動：** ←→方向鍵  
**旋轉：** ↑方向鍵  
**加速：** ↓方向鍵  
**落下：** 空白鍵  
**Hold方塊：** Z鍵  


## 亮點技術
* **7-bag：** 參考俄羅斯方塊標準隨機法，利用隨機演算法，確保每次方塊的隨機性，提升玩家遊戲體驗。  
```csharp

 void FillBag()
    {
        List<int> tempList = new List<int>();
        for (int i = 0; i < 7; i++) tempList.Add(i);    

        for(int i = 0; i < 7; i++)
        {
            int temp = tempList[i];
            int randomIndex = Random.Range(0, tempList.Count);
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            sevenBag.Add(tempList[i]);
        }

        if (nextPieceList.Count > 0) return;
        for (int i = 0; i < game.spriteCtrl.NextPiece.Length; i++)
        {
            nextPieceList.Add(sevenBag[0]);
            sevenBag.RemoveAt(0);
        }
    }
    int GetBag()
    {
        if (sevenBag.Count == 0) FillBag();

        int num = nextPieceList[0];
        nextPieceList.RemoveAt(0);

        nextPieceList.Add(sevenBag[0]);
        sevenBag.RemoveAt(0);
        return num;
    }
```

* **Wall Kick ：** 利用遞迴演算法實現基礎踢牆，未來將參考完整SRS系統優化。  
```csharp 
void TurnPieces()
    {
        if (!isMoving) return;
        bool canTurn = true;

        if (secondRotation) rotationIndex--;
        List<Vector2Int> TurnedGridPos = new List<Vector2Int>();

        if (!secondRotation)//紀錄I_Shape是否二次旋轉用
        {
            if (now_Shape == ShapePiece.I_shape) centerPos += I_ShapeCenterOffset[rotationIndex];
            if (now_Shape == ShapePiece.O_shape) return;
        }

        foreach (var pos in MovingPiecesOffset)
        {
            Vector2Int newPos = new Vector2Int(pos.y, -pos.x);
            Vector2Int tryTrunPos = newPos + centerPos;

            //判定是否撞牆或超出格子 -> 退位
            if (!grid.IsInside(tryTrunPos) || !grid.IsCellEmpty(tryTrunPos))
            {
                if (tryTrunPos.x < 0 || tryTrunPos.x >= grid.Width || !grid.IsCellEmpty(tryTrunPos))//左右退位
                {
                    if (tryTrunPos.x < centerPos.x) centerPos += new Vector2Int(1, 0);
                    if (tryTrunPos.x > centerPos.x) centerPos += new Vector2Int(-1, 0);
                }
                if (tryTrunPos.y < centerPos.y)//上下退位
                {
                    centerPos += new Vector2Int(0, 1);

                    //到底部時觸發底部計時邏輯
                    isCellMoveToBottom = false;
                    movingTimer = 0;
                    finalCellMoveTimer = 0.8f;
                }
                canTurn = false;
                secondRotation = true;
                TurnPieces();
            }
            else
            {
                TurnedGridPos.Add(newPos);
            }
        }
        //合法 -> 旋轉
        if (canTurn)
        {
            MovingPiecesOffset = TurnedGridPos;

            for (int i = 0; i < MovingPiecesObj.Count; i++)
            {
                Vector2Int cellgridPos = centerPos + MovingPiecesOffset[i];
                MovingPiecesObj[i].transform.position = grid.GridToWorldPos(cellgridPos);
            }

            secondRotation = false;
        }
        //I_shape 獨立旋轉模式
        if (now_Shape == ShapePiece.I_shape)
        {
            rotationIndex++;
            if (rotationIndex > 3) rotationIndex = 0;
        }
        
        PredictFallingPiece();

    }
```
## 開發目的
目前正在自學Unity C#，以練習演算法和物件導向為目的，同時也嘗試撰寫腳本時利用MVP觀念將畫面、數據及邏輯分開，確保未來方便維護與更新。

## 未來開發計畫
預計更新：  
* 連擊系統
* 分數系統  
* 完整SRS系統  
* ObjectPool優化  
* 美術  
 
