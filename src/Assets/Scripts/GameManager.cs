using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Rewired;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private float timerValue = 60f;

    [SerializeField]
    private float timerTickStep = 0.1f;

    // public PlanetGravity[] planets { get; set; }

    private Rewired.Player player;
    private TMP_Text timer;
    public PlayerController[] players { get; set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        player = ReInput.players.GetPlayer(0);

        // planets = GameObject.FindObjectsOfType<PlanetGravity>();

        players = GameObject.FindObjectsOfType<PlayerController>();

        timer = GameObject.Find("Canvas/Timer").GetComponent<TMP_Text>();

        timer.text = timerValue.ToString();

        // StartCoroutine(tickTimer());
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
