using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Rewired;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject playerPrefab;
    public float respawnTime = 3f;
    public float timeVisibleAfterDeath = 1f;
    public float respawnParticleDelay = 0.25f;
    public float timerValue = 60f;
    public float timerTickStep = 0.1f;
    public float cloudSpawnRate = 0.8f;
    public float cloudSpeedMin = 10f;
    public float cloudSpeedMax = 30f;
    public GameObject[] clouds;
    public float cloudXStart;
    public float cloudXEnd;
    public float cloudYOffset;

    public PointEffector2D[] planets { get; set; }
    public PlayerController[] players { get; set; }

    private Rewired.Player player;
    private TMP_Text timer;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            planets = GameObject.FindObjectsOfType<PointEffector2D>();

            for (var i = 0; i < Utilities.NumberOfPlayers; i++)
            {
                var index = Random.Range(0, GameManager.instance.planets.Length);

                var playerInstance = Instantiate(playerPrefab, instance.planets[index].transform.position, playerPrefab.transform.rotation);

                playerInstance.GetComponent<PlayerController>().playerID = i;
            }
        }


    }

    // Use this for initialization
    void Start()
    {
        player = ReInput.players.GetPlayer(0);


        players = GameObject.FindObjectsOfType<PlayerController>();

        timer = GameObject.Find("Canvas/Timer").GetComponent<TMP_Text>();

        timer.text = timerValue.ToString();

        // StartCoroutine(tickTimer());
        StartCoroutine(cloudSpawner());
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

    private IEnumerator cloudSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(cloudSpawnRate);

            var index = Random.Range(0, clouds.Length);

            var minY = Camera.main.transform.position.y - Camera.main.orthographicSize - cloudYOffset;
            var maxY = Camera.main.transform.position.y + Camera.main.orthographicSize + cloudYOffset;

            var pos = new Vector3(cloudXStart, Random.Range(minY, maxY), 0.0f);

            var cloud = Instantiate(clouds[index], pos, clouds[index].transform.rotation);

            var cloudController = cloud.GetComponent<CloudController>();
            cloudController.endX = cloudXEnd;
            cloudController.speed = Random.Range(cloudSpeedMin, cloudSpeedMax);
        }
    }
}
