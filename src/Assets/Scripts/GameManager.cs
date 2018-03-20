using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Rewired;
using EazyTools.SoundManager;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public float musicVolume = 0.5f;
    public float respawnTime = 3f;
    public float timeVisibleAfterDeath = 1f;
    public float respawnParticleDelay = 0.25f;
    public int timerValue = 60;
    public float cloudSpawnRate = 0.8f;
    public float cloudSpeedMin = 10f;
    public float cloudSpeedMax = 30f;
    public GameObject[] clouds;
    public float cloudXStart;
    public float cloudXEnd;
    public float cloudYOffset;

    [Header("Players")]
    public GameObject playerPrefab;
    public Sprite[] idleSprites;
    public RuntimeAnimatorController[] animatorControllers;

    [Header("Audio")]
    public AudioClip[] music;
    public AudioClip[] announcerNumbers;
    public AudioClip announcerBegin;
    public AudioClip announcerGameOver;

    public PointEffector2D[] Planets { get; set; }
    public PlayerController[] Players { get; set; }
    public bool GameStarted { get; set; } = false;
    public bool GameOver { get; set; } = false;

    private Rewired.Player player;
    private TMP_Text timer;
    private TMP_Text countdown;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            Planets = GameObject.FindObjectsOfType<PointEffector2D>();

            SpawnPlayers();
        }
    }

    // Use this for initialization
    void Start()
    {
        player = ReInput.players.GetPlayer(0);


        Players = GameObject.FindObjectsOfType<PlayerController>();

        timer = GameObject.Find("Canvas/Timer").GetComponent<TMP_Text>();
        countdown = GameObject.Find("Canvas/Countdown").GetComponent<TMP_Text>();

        timer.text = timerValue.ToString();
        countdown.text = "3";

        StartCoroutine(CloudSpawner());
        StartCoroutine(GameBeginCountdown());
        StartCoroutine(PlayMusic());
    }

    // Update is called once per frame
    void Update()
    {
        // if (player.GetButtonDown("Jump"))
        // {
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // }
    }

    private IEnumerator PlayMusic()
    {
        var musicID = SoundManager.PlayMusic(music[Random.Range(0, music.Length)], musicVolume, false, false);
        var musicAudio = SoundManager.GetMusicAudio(musicID);

        while (musicAudio.playing)
        {
            yield return null;
        }

        StartCoroutine(PlayMusic());
    }

    private IEnumerator TickTimer()
    {
        while (0 < timerValue)
        {
            yield return new WaitForSeconds(1);

            timerValue -= 1;

            timer.text = timerValue.ToString();
        }

        SoundManager.PlaySound(announcerGameOver);
        GameOver = true;
    }

    private IEnumerator GameBeginCountdown()
    {
        yield return new WaitForSecondsRealtime(1);
        SoundManager.PlaySound(announcerNumbers[2]);
        yield return new WaitForSecondsRealtime(1);
        SoundManager.PlaySound(announcerNumbers[1]);
        countdown.text = "2";
        yield return new WaitForSecondsRealtime(1);
        SoundManager.PlaySound(announcerNumbers[0]);
        countdown.text = "1";
        yield return new WaitForSecondsRealtime(1);
        countdown.text = "BEGIN";
        SoundManager.PlaySound(announcerBegin);
        yield return new WaitForSecondsRealtime(1);

        GameStarted = true;
        countdown.gameObject.SetActive(false);

        StartCoroutine(TickTimer());
    }

    private IEnumerator CloudSpawner()
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

    private void SpawnPlayers()
    {
        for (var i = 0; i < Utilities.NumberOfPlayers; i++)
        {
            var index = Random.Range(0, GameManager.instance.Planets.Length);

            var playerInstance = Instantiate(playerPrefab, instance.Planets[index].transform.position, playerPrefab.transform.rotation);

            var playerController = playerInstance.GetComponent<PlayerController>();
            playerController.playerID = i;
            playerController.IdleSprite = idleSprites[i];
            playerController.AnimatorController = animatorControllers[i];
        }
    }
}
