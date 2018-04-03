using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public int playerCount;
    public Button playButton;
    public Button playersButton;
    public Text playersLabel;
    void Start()
    {
        playerCount = 2;
        // playButton.onClick.AddListener(play);
        // playersButton.onClick.AddListener(updatePlayers);
    }
    public void play()
    {
        Utilities.NumberOfPlayers = playerCount;
        SceneManager.LoadScene(1); // This should be the GAME index
    }

    public void updatePlayers()
    {
        Debug.Log("CLICK");
        playerCount = playerCount == 4 ? 2 : playerCount + 1;
        playersLabel.text = "Players: " + playerCount.ToString();
    }
}
