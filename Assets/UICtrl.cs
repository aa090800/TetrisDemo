using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    public GameObject StartPanel;
    public GameObject PausePanel;
    public GameObject FailedPanel;

    gameManager game;
    private void Start()
    {
        game = gameManager.instance;
    }



    

    public void Btn_GameStart()
    {
        game.cell.SetState_Playing();
        StartPanel.SetActive(false);
        PausePanel.SetActive(false);
    }
    public void Btn_GameRestart()
    {
        game.cell.ResetGame();
        StartPanel.SetActive(false);
        PausePanel.SetActive(false);
        FailedPanel.SetActive(false);
    }


    public void Btn_GameExit()
    {
        Application.Quit();
    }
    public void GameResume()
    {
        game.cell.SetState_Playing();
        PausePanel.SetActive(false);
    }
    public void GamePause()
    {
        PausePanel.SetActive(true);
    }
    public void GameTitle()
    {
        StartPanel.SetActive(true);
        PausePanel.SetActive(false);
        FailedPanel.SetActive(false);
    }
    public void GameOver()
    {
        FailedPanel.SetActive(true);
    }

}
