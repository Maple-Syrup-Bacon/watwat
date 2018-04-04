using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DoozyUI;

public class MenuManager : MonoBehaviour
{
    public TMP_Text playersLabel;
    public TMP_Text winScoreLabel;
    public int[] winScores;
    public int defaultWinScoreIndex;
    private int playerCount;

    private void Start()
    {
        playerCount = 2;
        Time.timeScale = 1;
    }

    public void Play()
    {
        Utilities.NumberOfPlayers = playerCount;
        Utilities.WinScore = winScores[defaultWinScoreIndex];
        SceneManager.LoadScene(1); // This should be the GAME index
    }

    public void UpdatePlayers()
    {
        playerCount = playerCount == 4 ? 2 : playerCount + 1;
        playersLabel.text = "Players: " + playerCount.ToString();
    }

    public void UpdateWinScore()
    {
        defaultWinScoreIndex++;
        defaultWinScoreIndex = defaultWinScoreIndex % winScores.Length;

        winScoreLabel.text = "Win Score: " + winScores[defaultWinScoreIndex].ToString();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
