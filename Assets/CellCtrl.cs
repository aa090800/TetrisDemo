using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * 
note:
帶修正:
最上方不能旋轉 V
撞牆不會退格 V
I和O的中心位置修正 V
失敗條件 V

手感部分:
按下落到最下方時 長一點點重計時 自然落下時短一點點重計時 V
暗助左右移動速度調整

bug:
方塊在下方不會退格 V



代新增:
按下加速 V
案左右加速 V
落下預判點 V
消行 V
hold方塊 V
預顯示下一個方塊 V
分數
combo
難度隨消的次數增加便難(可選)
介面 A


*/



enum ShapePiece { I_shape,O_shape,J_shape,L_shape,S_shape,T_shape,Z_shape}
enum GameState { Title, Playing, Pause, GameOver }

public class CellCtrl : MonoBehaviour
{

    [Header("Manager")]
    public GridManager grid;
    public gameManager game;

    [Header("Prefab")]
    public GameObject CellPrefab;

    //核心資料
    GameState nowState;
    Dictionary<Vector2Int, GameObject> piecesDictionary = new Dictionary<Vector2Int, GameObject>();
    //遊戲狀態
    Vector2Int StartCenterPos;
    Vector2Int centerPos;
    public float fallingSpeed;



    //====方塊控制====
    //移動
    ShapePiece now_Shape;
    List<Vector2Int> NextPlace = new List<Vector2Int>();
    List<GameObject> MovingPiecesObj = new List<GameObject>();
    List<Vector2Int> MovingPiecesOffset = new List<Vector2Int>();
    bool isMoving;
    float movingTimer;
    bool readyToSpawnPiece = true;
    float inputTimer;

    //預判落點
    GameObject[] fallingPieces;



    //管理觸底秒數
    bool naturalFall;
    bool downFall;
    bool isCellMoveToBottom;
    float finalCellMoveTimer = 0;
    float endlessFallingTimer = 0;

    //7-bag & hold方塊
    List<int> sevenBag = new List<int>();
    List<int> nextPieceList = new List<int>();//只有5個
    bool pieceHolded;
    bool firstHold;
    int holdedNum = 0;

    //旋轉
    int rotationIndex = 0;
    bool secondRotation = false;


    //方塊生成形狀
    List<Vector2Int> I_list, O_list, J_list, L_list, S_list, T_list, Z_list;
    Dictionary<ShapePiece, List<Vector2Int>> shapesOffset;
    
    private void Start()
    {
        game = gameManager.instance;
        //初始化畫面
        SetState_Title();
        SpawnWall();
        game.spriteCtrl.HoldedPieceInvincible();
        //初始化起點
        StartCenterPos = new Vector2Int(5, 20);
        centerPos = StartCenterPos;
        
        SetShape();
        FillBag();
        SpawnPiece(GetBag());
        ShowNextPieceSprite();
    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetState_PauseofPlaying();
        }



        if (nowState != GameState.Playing) return;
        EndlessTimeChecker();
        CellMove();
        TryPiecesMove();
        if (Input.GetKeyDown(KeyCode.Space))//直接下墜到底
        {
            DropStrightly();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TurnPieces();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            TryPieceHold();
        }
    }

    public void Init(GridManager grid)
    {
        this.grid = grid;
    }

    #region ===========包裝方法============

    void EndlessTimeChecker()
    {
        if (isCellMoveToBottom)
        {
            endlessFallingTimer += Time.deltaTime;
            if (endlessFallingTimer < 1) return;
            DropStrightly();
            endlessFallingTimer = 0;
        }
    }
    void CellMove()
    {
        if (!isCellMoveToBottom)//正常下墜
        {
            movingTimer += Time.deltaTime;
            if (movingTimer < 1 / fallingSpeed) return;
            movingTimer = 0;
            naturalFall = true;
        }
        else//到底
        {
            movingTimer += Time.deltaTime;
            if (movingTimer < finalCellMoveTimer) return;
            movingTimer = 0;
        }

        PiecesMove(Vector2Int.down);
    }

    #endregion

    #region=================移動==================
    //最後一格 -> 自然下墜來的0.5秒/手動下來的0.8秒 移動過後重計0.5秒 -> 移走變成正常下墜 -> 但避免卡無限時間 碰過底有1秒可以扣 

    void PiecesMove(Vector2Int dir)
    {
        if (!isMoving) return;
        if (isCellMoveToBottom && (dir == Vector2Int.left || dir == Vector2Int.right))
        {
            movingTimer = 0;
            finalCellMoveTimer = 0.8f;
        }
        if (isCellMoveToBottom && dir == Vector2Int.down) return;
            //檢查左右不能超出 之後檢查最底是不是超界或形狀撞到方塊
            NextPlace.Clear();
        for (int i = 0; i < MovingPiecesObj.Count; i++)//判斷其中一片下一格出界
        {
            NextPlace.Add(centerPos + MovingPiecesOffset[i] + dir);//下一個落點
        }

        bool canmove = true;

        foreach(var pos in NextPlace)
        {
            if ((!grid.IsInside(pos) || !grid.IsCellEmpty(pos)))
            {
                if (dir == Vector2Int.down)
                {
                    canmove = false;
                    break;
                }
                else return;                    
            }
        }


        if (canmove)
        { //通過才移動
            
            centerPos += dir;
            for (int i = 0; i < MovingPiecesObj.Count; i++)
            {
                Vector2Int cellgridPos = centerPos + MovingPiecesOffset[i];
                MovingPiecesObj[i].transform.position = grid.GridToWorldPos(cellgridPos);
            }
            //到底時秒數更變
            if (naturalFall || downFall)
            {
                List<Vector2Int> tempPos = new List<Vector2Int>();

                for (int i = 0; i < MovingPiecesObj.Count; i++)
                {
                    tempPos.Add(centerPos + MovingPiecesOffset[i] + Vector2Int.down);
                    if (!grid.IsCellEmpty(tempPos[i]))
                    {
                        if (naturalFall) finalCellMoveTimer = 0.5f;
                        else finalCellMoveTimer = 0.8f;
                        isCellMoveToBottom = true;
                        break;//最底了
                    }
                }
            }
            else
            {
                isCellMoveToBottom = false;
                movingTimer = 0;
            }

        }
        else
        {
            for (int j = 0; j < NextPlace.Count; j++)
            {
                grid.SetPieceFilled(centerPos + MovingPiecesOffset[j]);
            }
            FinishCellFall();
            SpawnPiece(GetBag());
            //自動生成下一塊
            return;
        }
        PredictFallingPiece();
    }

    //直接下墜
    void DropStrightly()
    {
        while (isMoving)
        {
            NextPlace.Clear();
            bool canMove = true;


            for (int i = 0; i < MovingPiecesObj.Count; i++)
            {
                NextPlace.Add(centerPos + MovingPiecesOffset[i] + Vector2Int.down);

                if (NextPlace[i].y < 0 || !grid.IsCellEmpty(NextPlace[i]))
                {
                    canMove = false;
                    break;
                }
            }

            if (canMove)
            {
                centerPos += Vector2Int.down;
            }
            else
            {
                for (int i = 0; i < MovingPiecesObj.Count; i++)
                {
                    grid.SetPieceFilled(centerPos + MovingPiecesOffset[i]);
                    //更新位置
                    Vector2Int cellgridPos = centerPos + MovingPiecesOffset[i];
                    MovingPiecesObj[i].transform.position = grid.GridToWorldPos(cellgridPos);
                }
                FinishCellFall();
            }
        }
        SpawnPiece(GetBag());

    }

    //方塊落到最底時
    void FinishCellFall()
    {
        foreach(var obj in MovingPiecesObj)
        {
            piecesDictionary.Add(grid.WorldToGridPos(obj.transform.position), obj);
        }

        TryLineClear();

        for(int i = 0; i < MovingPiecesObj.Count; i++)
        {
            Vector2Int pos = centerPos + MovingPiecesOffset[i];
        }
        
        isMoving = false;
        isCellMoveToBottom = false;
        MovingPiecesObj.Clear();
        MovingPiecesOffset.Clear();
        centerPos = StartCenterPos;
        readyToSpawnPiece = true;
        firstHold = false;

        if(pieceHolded) game.spriteCtrl.HoldedPieceBecomeNormal();

    }

    //按住方向自動移動
    void TryPiecesMove()
    {
        
        if (Input.anyKeyDown)
        {
            Vector2Int dir = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.RightArrow)) dir = Vector2Int.right;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) dir = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                downFall = true;
                dir = Vector2Int.down;
            }
            inputTimer = 0;
            PiecesMove(dir);
        }

        if (Input.anyKey)
        {
            Vector2Int dir = Vector2Int.zero;
            if (Input.GetKey(KeyCode.RightArrow)) dir = Vector2Int.right;
            if (Input.GetKey(KeyCode.LeftArrow)) dir = Vector2Int.left;
            if (Input.GetKey(KeyCode.DownArrow))
            {
                downFall = true;
                dir = Vector2Int.down;
            }
            inputTimer += Time.deltaTime;
            if (inputTimer < 0.2f) return;
            inputTimer -= 0.05f;
            PiecesMove(dir);
        }
    }
    #endregion

    #region====================生成方塊=====================
    void SpawnPiece(int num)//抓bag 取出的數字當作形狀生成方塊
    {
        

        //先隨機0~6圖案 之後再改queue
        //pos -> pos跑形狀
        now_Shape = (ShapePiece)num;
        List<Vector2Int> nextShape = shapesOffset[now_Shape];
        foreach(Vector2Int pos in nextShape)
        {
            GameObject go= Instantiate(CellPrefab, grid.GridToWorldPos(pos+centerPos), Quaternion.identity);

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.color=SetColor(now_Shape); 

            MovingPiecesObj.Add(go);
            MovingPiecesOffset.Add(pos);

        }
        isCellMoveToBottom = false;
        rotationIndex = 0;
        readyToSpawnPiece = false;
        isMoving = true;

        TryGameOver();

        PredictFallingPiece();
        ShowNextPieceSprite();
    }
    void ShowNextPieceSprite()//顯示5個方塊圖示
    {
        List<int> tempList = new List<int>();
        for (int i = 0; i < game.spriteCtrl.NextPiece.Length; i++)
        {
            tempList.Add(nextPieceList[i]);
        }
        game.spriteCtrl.ShowNextPiece(tempList);
    }
    #endregion

    #region================預判落點==================
    //抓現在的movingPieces
    //    
    void PredictFallingPiece()
    {
        if (fallingPieces != null)//每次移動刪除預判格子 
        {
            foreach(var obj in fallingPieces)
            {
                Destroy(obj);
            }
        }

        Vector2Int tempCenterpos = centerPos;
        bool isBottom = false;

        //邏輯: 中心點往下掉落1格 -> 判定是否到底 -> (是)生成預判方塊 (否)繼續往下+1判定
        while (true)
        {
            List<Vector2Int> nextPos = new List<Vector2Int>();
            for (int i = 0; i < MovingPiecesObj.Count; i++)
            {
                Vector2Int pos = tempCenterpos + MovingPiecesOffset[i] + Vector2Int.down;
                nextPos.Add(pos);
                if (pos.y < 0 || !grid.IsCellEmpty(pos))
                {
                    isBottom = true;
                    break;
                }
            }
            if (!isBottom)
            {
                tempCenterpos += Vector2Int.down;
                continue;
            }
            else
            {
                fallingPieces = new GameObject[MovingPiecesObj.Count];
                for(int i = 0; i < MovingPiecesObj.Count; i++)
                {
                    fallingPieces[i]= Instantiate(CellPrefab, transform.position, Quaternion.identity);
                    GameObject go = fallingPieces[i];
                    Vector2Int pos = tempCenterpos + MovingPiecesOffset[i];
                    go.transform.position = grid.GridToWorldPos(pos);

                    SpriteRenderer sr= go.GetComponent<SpriteRenderer>();
                    sr.color = new Color(0.5f,0.5f,0.5f,0.5f);
                    sr.sortingOrder = -1;
                }
                break;
            }
        }        
    }

    #endregion

    #region================失敗判定===================
    void TryGameOver()
    {
        //Debug.Log("test");
        for(int i = 0; i < MovingPiecesObj.Count; i++)
        {
            Vector2Int pos = centerPos + MovingPiecesOffset[i];
            if (!grid.IsCellEmpty(pos))
            {
                GameFail();
                return;
            }
        }
    }
    #endregion

    #region=================7-Bag================
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
    int GetBag()//取得數字並-1
    {
        if (sevenBag.Count == 0) FillBag();//歸零自動+7 bag

        int num = nextPieceList[0];
        nextPieceList.RemoveAt(0);

        nextPieceList.Add(sevenBag[0]);
        sevenBag.RemoveAt(0);
        return num;
    }

    #endregion

    #region=============HOLD方塊================
    void TryPieceHold()
    {
        if (firstHold)return;
        if (pieceHolded)
        {
            int tempNum = holdedNum;
            holdedNum = (int)now_Shape;

            DeletePiece();
            SpawnPiece(tempNum);
            firstHold = true;
        }
        else
        {

            DeletePiece();
            holdedNum = (int)now_Shape;
            SpawnPiece(GetBag());
        }
        if (!pieceHolded) pieceHolded = true;
        firstHold = true;
        game.spriteCtrl.HoldedPieceBecomeGray();


        game.spriteCtrl.ShowHoldedPiece(holdedNum);
    }

    void DeletePiece()
    {
        foreach (var go in MovingPiecesObj)
        {
            Destroy(go);
        }
        MovingPiecesObj.Clear();
        MovingPiecesOffset.Clear();
        centerPos = StartCenterPos;
    }

    #endregion

    #region=============旋轉===============
    //(x,y)->(y,-x)
    void TurnPieces()
    {
        if (!isMoving) return;
        bool canTurn = true;

        if (secondRotation) rotationIndex--;
        List<Vector2Int> TurnedGridPos = new List<Vector2Int>();

        if (!secondRotation)
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

    #endregion

    #region    ============消行============
    //y=0開始偵測 每個X都是CellState.Filled 滿足消行 刪除 ->可優化成piece形狀的最下面那行Y開始偵測
    //y+1以上所有方塊下墜1格 此處可加動畫
    //(0,1)->(9,1) list<gameobject> obj .add ->foreach obj 格子往下移一個gird.gridToWorld(Vector2Int.down)->grid.ChangeCellState(原點,下一點)
    //之後(1,1)->(9,1)以上也要跑一次剛剛的全部
    //再來從剛剛消行那個y +1往上偵測是否滿足消行 至下一個y行全部X都是0結束

    void TryLineClear()
    {
        
        int StartY = centerPos.y;
        foreach(var pos in MovingPiecesOffset)
        {            
            Vector2Int vec = centerPos + pos;
            if (vec.y <= StartY) StartY = vec.y;            
        }
        for(int lineY = StartY; lineY < grid.Height; lineY++)
        {
            //檢查至全空那行中斷
            int topIndex = 0;
            for (int x = 0; x < grid.Width; x++) if (grid.IsCellEmpty(new Vector2Int(x, lineY))) topIndex++;
            if (topIndex == 10) break;


            int fullCount = 0;
            for(int x = 0; x < grid.Width; x++)//檢查是否滿行
            {
                if (!grid.IsCellEmpty(new Vector2Int(x, lineY))) fullCount++;
            }
            if (fullCount == grid.Width)//滿足一行
            {
                for (int lineX = 0; lineX < grid.Width; lineX++)
                {
                    //刪除整行
                    grid.SetPieceEmpty(new Vector2Int(lineX, lineY));
                    Destroy(piecesDictionary[new Vector2Int(lineX, lineY)]);
                    piecesDictionary.Remove(new Vector2Int(lineX, lineY));
                }
                //預留消除動畫空間



                //下落一格
                for (int y = lineY + 1; y < grid.Height; y++)//height 可直到最高點(都是空的那行)
                {                   
                    for (int x = 0; x < grid.Width; x++)
                    {
                        Vector2Int UpperPos = new Vector2Int(x, y);
                        Vector2Int LowerPos = new Vector2Int(x, y - 1);
                        if (grid.IsCellEmpty(UpperPos)) continue;

                        //物理上往下掉一格
                        GameObject go = piecesDictionary[UpperPos];
                        if (!grid.IsCellEmpty(UpperPos))
                        {
                            grid.SetPieceEmpty(UpperPos);
                            grid.SetPieceFilled(LowerPos);
                        }
                        piecesDictionary.Remove(UpperPos);
                        go.transform.position = grid.GridToWorldPos(LowerPos);
                        piecesDictionary.Add(grid.WorldToGridPos(go.transform.position), go);
                        
                    }
                    
                }
                //全部往下掉結束
            }
            else continue;
            lineY--;
        }
    }


    void SetShape()
    {
        shapesOffset = new Dictionary<ShapePiece, List<Vector2Int>>();
        I_list = new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(-2, 0),new Vector2Int(0,0) };
        T_list = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 0) };
        Z_list = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(-1, 1), new Vector2Int(1, 0), new Vector2Int(0, 0) };
        S_list = new List<Vector2Int> { new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(0, 0) };
        O_list = new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0) };
        J_list = new List<Vector2Int> { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 0) };
        L_list = new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 0) };
        shapesOffset.Add(ShapePiece.I_shape, I_list);
        shapesOffset.Add(ShapePiece.T_shape, T_list);
        shapesOffset.Add(ShapePiece.O_shape, O_list);
        shapesOffset.Add(ShapePiece.S_shape, S_list);
        shapesOffset.Add(ShapePiece.Z_shape, Z_list);
        shapesOffset.Add(ShapePiece.J_shape, J_list);
        shapesOffset.Add(ShapePiece.L_shape, L_list);
    }
    Color SetColor(ShapePiece shape)
    {
        System.Drawing.Color color;
        switch (shape)
        {
            case ShapePiece.I_shape: color = System.Drawing.Color.SkyBlue;break;
            case ShapePiece.T_shape: color = System.Drawing.Color.Pink; break;
            case ShapePiece.O_shape: color = System.Drawing.Color.Gold; break;
            case ShapePiece.S_shape: color = System.Drawing.Color.SpringGreen; break;
            case ShapePiece.Z_shape: color = System.Drawing.Color.Crimson; break;
            case ShapePiece.L_shape: color = System.Drawing.Color.RoyalBlue; break;
            case ShapePiece.J_shape: color = System.Drawing.Color.SandyBrown; break;
            default: color = System.Drawing.Color.White;break;
        }
        return new Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }

    void SpawnWall()
    {
        List<Vector3> spawnPos = new List<Vector3>();
        for(int i = 0; i < 20; i++)
        {
            Vector3 posL = grid.GridToWorldPos(new Vector2Int(-1, i));
            Vector3 posR =grid.GridToWorldPos(new Vector2Int(grid.Width, i));
            spawnPos.Add(posL);
            spawnPos.Add(posR);
        }
        for(int i = -1; i < grid.Width + 1; i++)
        {
            Vector3 pos = grid.GridToWorldPos(new Vector2Int(i, -1));
            spawnPos.Add(pos);
        }
        foreach(Vector3 pos in spawnPos)
        {
            GameObject go = Instantiate(CellPrefab, pos, Quaternion.identity);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.color = Color.gray;
        }

    }

    Vector2Int[] I_ShapeCenterOffset = new Vector2Int[]
    {
        new Vector2Int(0,-1),new Vector2Int(-1,0),new Vector2Int(0,1),new Vector2Int(1,0)
    };

    #endregion

    #region=========ResetGame==========
    public void ResetGame()
    {
        for(int x = 0; x < grid.Width; x++)
        {
            for(int y = 0; y < grid.Height; y++)
            {
                grid.SetPieceEmpty(new Vector2Int(x, y));
            }
        }
        foreach(var obj in MovingPiecesObj)
        {
            Destroy(obj);
        }
        foreach(var obj in piecesDictionary)
        {
            Destroy(obj.Value);
        }
        holdedNum = 0;
        firstHold = false;
        pieceHolded=false;

        MovingPiecesObj.Clear();
        MovingPiecesOffset.Clear();
        sevenBag.Clear();
        piecesDictionary.Clear();

        SpawnWall();
        game.spriteCtrl.HoldedPieceInvincible();
        SetShape();
        StartCenterPos = new Vector2Int(5, 20);
        centerPos = StartCenterPos;

        FillBag();
        SpawnPiece(GetBag());
        ShowNextPieceSprite();

        nowState = GameState.Playing;
    }

    void GameFail()
    {
        nowState = GameState.GameOver;
        game.ui.GameOver();
    }

    #endregion

    #region========Button============
    public void SetState_Playing()
    {
        nowState = GameState.Playing;   
    }
    public void SetState_Title()
    {
        game.ui.GameTitle();
        nowState = GameState.Title;
    }
    public void SetState_GameOver()
    {
        nowState = GameState.GameOver;
    }
    public void SetState_PauseofPlaying()
    {
        game.ui.GamePause();

        nowState = (nowState == GameState.Playing) ? GameState.Pause : GameState.Playing;

    }
    #endregion
}
