using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_Text playersLabel;

    private int playerCount;

    private void Start()
    {
        playerCount = 2;
    }

    public void Play()
    {
        Utilities.NumberOfPlayers = playerCount;
        SceneManager.LoadScene(1); // This should be the GAME index
    }

    public void UpdatePlayers()
    {
        playerCount = playerCount == 4 ? 2 : playerCount + 1;
        playersLabel.text = "Players: " + playerCount.ToString();
    }
}
