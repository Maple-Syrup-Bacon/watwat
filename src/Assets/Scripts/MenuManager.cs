using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadGameWithPlayers(int players)
    {
        Utilities.NumberOfPlayers = players;
        SceneManager.LoadScene(1); // This should be the GAME index
    }
}
