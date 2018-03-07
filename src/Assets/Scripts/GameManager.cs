using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Rewired;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float timerValue = 60f;
    [SerializeField]
    private float timerTickStep = 0.1f;
    private Rewired.Player player;
    private TMP_Text timer;

    // Use this for initialization
    void Start()
    {
        player = ReInput.players.GetPlayer(0);
        timer = GameObject.Find("Canvas/Timer").GetComponent<TMP_Text>();

        timer.text = timerValue.ToString();

        StartCoroutine(tickTimer());
    }

    // Update is called once per frame
    void Update()
    {
        // if (player.GetButtonDown("Jump"))
        // {
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // }
    }

    public void SubtractTime(float t)
    {
        timerValue -= t;
    }

    private IEnumerator tickTimer()
    {
        while (0f < timerValue)
        {
            yield return new WaitForSeconds(timerTickStep);

            timerValue -= timerTickStep;

            timer.text = timerValue.ToString("N1");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
