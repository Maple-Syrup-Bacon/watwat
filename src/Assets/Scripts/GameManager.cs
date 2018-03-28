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

    public float baseKnockback = 10f;
    public float musicVolume = 0.5f;
    public float respawnTime = 3f;
    public float timeVisibleAfterDeath = 1f;
    public float respawnParticleDelay = 0.25f;
    public int timerValue = 60;
    public float cloudSpawnRate = 0.8f;
    public float cloudSpeedMin = 10f;
    public float cloudSpeedMax = 30f;
    public GameObject[] clouds;
    public float cloudRotationSpeedMin = 5f;
    public float cloudRotationSpeedMax = 30f;
    public float cloudXStart;
    public float cloudXEnd;
    public float cloudYOffset;

    [Header("Players")]
    public GameObject playerPrefab;
    public Sprite[] idleSprites;
    public Color[] playerColors;
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
    private TMP_Text[] percentages;
    private TMP_Text[] scores;

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
            GetUIElements();
        }
    }

    // Use this for initialization
    void Start()
    {
        player = ReInput.players.GetPlayer(0);

        timer.text = timerValue.ToString();
        countdown.text = "3";

        UpdateAvatars();

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
        countdown.gameObject.SetActive(true);
        countdown.text = "GAME OVER";
        yield return new WaitForSecondsRealtime(5.0f);
        SceneManager.LoadScene(0);
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

            var rotSpeed = Random.Range(cloudRotationSpeedMin, cloudRotationSpeedMax);
            rotSpeed = (Random.Range(0, 2) == 0 ? rotSpeed : -rotSpeed);
            cloudController.rotationSpeed = rotSpeed;
        }
    }

    private void SpawnPlayers()
    {
        Players = new PlayerController[Utilities.NumberOfPlayers];

        for (var i = 0; i < Utilities.NumberOfPlayers; i++)
        {
            var index = Random.Range(0, GameManager.instance.Planets.Length);

            var playerInstance = Instantiate(playerPrefab, instance.Planets[index].transform.position, playerPrefab.transform.rotation);

            var playerController = playerInstance.GetComponent<PlayerController>();
            playerController.playerID = i;
            playerController.IdleSprite = idleSprites[i];
            playerController.AnimatorController = animatorControllers[i];

            var trailRenderer = playerInstance.GetComponent<TrailRenderer>();
            trailRenderer.startColor = playerColors[i];
            var endColor = new Color();
            endColor.r = playerColors[i].r;
            endColor.g = playerColors[i].g;
            endColor.b = playerColors[i].b;
            endColor.a = 0;
            trailRenderer.endColor = endColor;

            Players[i] = playerController;
        }
    }

    private void GetUIElements()
    {
        timer = GameObject.Find("Canvas/Timer").GetComponent<TMP_Text>();
        countdown = GameObject.Find("Canvas/Countdown").GetComponent<TMP_Text>();

        percentages = new TMP_Text[Utilities.NumberOfPlayers];
        scores = new TMP_Text[Utilities.NumberOfPlayers];

        var avatars = GameObject.Find("Canvas/Avatars").transform;

        if (Utilities.NumberOfPlayers == 2)
        {
            avatars.GetChild(2).gameObject.SetActive(false);
            avatars.GetChild(3).gameObject.SetActive(false);
        }
        else if (Utilities.NumberOfPlayers == 3)
        {
            avatars.GetChild(3).gameObject.SetActive(false);
        }

        for (var i = 0; i < Utilities.NumberOfPlayers; i++)
        {
            var avatar = avatars.GetChild(i);
            percentages[i] = avatar.GetChild(0).GetComponent<TMP_Text>();
            scores[i] = avatar.GetChild(1).GetComponent<TMP_Text>();
        }
    }

    public void UpdateAvatars()
    {
        for (var i = 0; i < Utilities.NumberOfPlayers; i++)
        {
            percentages[i].text = Players[i].damageTotal + "%";
            scores[i].text = Players[i].score.ToString();
        }
    }

    public void IncreaseScore(int playerID)
    {
        if (GameOver)
        {
            return;
        }

        Players[playerID].score++;
        UpdateAvatars();
    }
}
