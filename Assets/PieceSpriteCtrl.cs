using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpriteCtrl : MonoBehaviour
{
    public Sprite[] ShapeSprite;

    public GameObject[] NextPiece;

    public GameObject HoldingPiece;

    public gameManager game;
    // Start is called before the first frame update
    void Start()
    {
        game = gameManager.instance;
    }

    public void ShowNextPiece(List<int> list)//5個的list 對應nextpiece的數量
    {
        for(int i = 0; i < NextPiece.Length; i++)
        {
            SpriteRenderer sr = NextPiece[i].GetComponent<SpriteRenderer>();
            sr.sprite = ShapeSprite[list[i]];
        }
    }

    public void ShowHoldedPiece(int num)
    {
        SpriteRenderer sr = HoldingPiece.GetComponent<SpriteRenderer>();
        sr.sprite = ShapeSprite[num];
    }
    public void HoldedPieceBecomeGray()
    {
        SpriteRenderer sr = HoldingPiece.GetComponent<SpriteRenderer>();
        sr.color =Color.gray;
    }
    public void HoldedPieceBecomeNormal()
    {
        SpriteRenderer sr = HoldingPiece.GetComponent<SpriteRenderer>();
        sr.color =Color.white;
    }
    public void HoldedPieceInvincible()
    {
        SpriteRenderer sr = HoldingPiece.GetComponent<SpriteRenderer>();
        sr.color=new Color(1,1,1,0);
    }
}
