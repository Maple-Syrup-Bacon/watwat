using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using Rewired.Integration.UnityUI;
using EazyTools.SoundManager;
using DoozyUI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int winScore = 5;
    [Tooltip("When a player dies the last one to damage gets the credit for the kill, but that damage must have been within a specific amount of seconds or it is counted as a suicide.")]
    public float lastTouchDuration = 5f;
    public float baseKnockback = 10f;
    public float musicVolume = 0.5f;
    public float respawnTime = 3f;
    public float timeVisibleAfterDeath = 1f;
    public float respawnParticleDelay = 0.25f;
    public int timerValue = 60;

    [Header("Players")]
    public GameObject playerPrefab;
    public Vector2[] initialSpawnPoints;
    public Sprite[] winSprites;
    public Sprite[] idleSprites;
    public Color[] playerColors;
    public RuntimeAnimatorController[] animatorControllers;

    [Header("Audio")]
    public AudioClip[] music;
    public AudioClip[] announcerNumbers;
    public AudioClip announcerGameOver;

    public PointEffector2D[] Planets { get; set; }
    public PlayerController[] Players { get; set; }
    public Light[] PlayerLights { get; set; }
    public bool GameStarted { get; set; } = false;
    public bool GameOver { get; set; } = false;
    public bool Paused { get; set; } = false;

    private RewiredStandaloneInputModule uiInputModule;
    // private TMP_Text timer;
    private TMP_Text countdown;
    private TMP_Text[] percentages;
    private TMP_Text[] scores;
    private Image winnerSprite;
    private EventSystem eventSystem;

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

    private void Start()
    {
        UIManager.HideUiElement("Background", "WATWAT", true);
        UIManager.HideUiElement("GameOverMenu", "WATWAT", true);
        UIManager.HideUiElement("PauseMenu", "WATWAT", true);

        Resume();

        winScore = Utilities.WinScore;

        countdown.text = "3";

        StartCoroutine(GameBeginCountdown());
        StartCoroutine(PlayMusic());
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

    private IEnumerator GameBeginCountdown()
    {
        var cix = 3;
        while (0 <= cix)
        {
            yield return new WaitForSeconds(1);
            SoundManager.PlaySound(announcerNumbers[cix]);
            countdown.text = cix == 0 ? "BEGIN" : cix.ToString();
            cix--;
        }
        yield return new WaitForSeconds(1);

        GameStarted = true;
        countdown.gameObject.SetActive(false);
    }

    private void SpawnPlayers()
    {
        Players = new PlayerController[Utilities.NumberOfPlayers];
        PlayerLights = new Light[Utilities.NumberOfPlayers];

        for (var i = 0; i < Utilities.NumberOfPlayers; i++)
        {
            var playerInstance = Instantiate(playerPrefab, initialSpawnPoints[i], playerPrefab.transform.rotation);

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

            PlayerLights[i] = playerInstance.transform.Find("Light").GetComponent<Light>();

            Players[i] = playerController;
        }
    }

    private void GetUIElements()
    {
        winnerSprite = GameObject.Find("MasterCanvas/UIGameOverMenu/Winner/Sprite").GetComponent<Image>();
        eventSystem = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<EventSystem>();
        uiInputModule = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<Rewired.Integration.UnityUI.RewiredStandaloneInputModule>();
        countdown = GameObject.Find("MasterCanvas/UICountdown/Text").GetComponent<TMP_Text>();

        uiInputModule.UseAllRewiredGamePlayers = false;
        uiInputModule.RewiredPlayerIds = new int[0];

        percentages = new TMP_Text[Utilities.NumberOfPlayers];
        scores = new TMP_Text[Utilities.NumberOfPlayers];

        var avatars = GameObject.Find("MasterCanvas/UIAvatars/Group").transform;

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

        // Set Avatar numbers
        for (var i = 0; i < Utilities.NumberOfPlayers; i++)
        {
            percentages[i].text = Players[i].damageTotal + "%";
            scores[i].text = Players[i].score.ToString();
        }
    }

    private void Pause()
    {
        Paused = true;
        Time.timeScale = 0;
        UIManager.ShowUiElement("Background", "WATWAT", false);
        UIManager.ShowUiElement("PauseMenu", "WATWAT", false);
    }

    public void SetAvatarPercentage(int playerID, int value)
    {
        percentages[playerID].text = Players[playerID].damageTotal + "%";
        percentages[playerID].transform.DOShakePosition(0.25f, 50, 50);
    }

    public void IncreaseScore(int playerID)
    {
        if (GameOver)
        {
            return;
        }

        Players[playerID].score++;

        StartCoroutine(IncreaseScoreAnimation(playerID));

        if (!GameOver && winScore <= Players[playerID].score)
        {
            SoundManager.PlaySound(announcerGameOver);
            GameOver = true;
            UIManager.ShowUiElement("Background", "WATWAT", false);
            UIManager.ShowUiElement("GameOverMenu", "WATWAT", false);
            winnerSprite.sprite = winSprites[playerID];
            uiInputModule.UseAllRewiredGamePlayers = true;
        }
    }

    public void DecrementScore(int playerID)
    {
        if (GameOver || Players[playerID].score == 0)
        {
            return;
        }

        Players[playerID].score--;

        StartCoroutine(IncreaseScoreAnimation(playerID));
    }

    private IEnumerator IncreaseScoreAnimation(int playerID)
    {
        yield return new WaitForSeconds(0.2f);
        scores[playerID].transform.DOPunchScale(new Vector3(10, 10, 10), 1f, 4, 0.4f);
        scores[playerID].transform.DOJump(scores[playerID].transform.position, 200f, 1, 1f);
        yield return new WaitForSeconds(0.5f);
        scores[playerID].text = Players[playerID].score.ToString();
    }

    private IEnumerator DecreaseScoreAnimation(int playerID)
    {
        yield return new WaitForSeconds(0.2f);
        scores[playerID].transform.DOPunchScale(new Vector3(10, 10, 10), 1f, 4, 0.4f);
        scores[playerID].transform.DOPunchRotation(new Vector3(10, 10, 10), 1f, 4, 0.5f);
        scores[playerID].transform.DOJump(scores[playerID].transform.position, 200f, 1, 1f);
        yield return new WaitForSeconds(0.5f);
        scores[playerID].text = Players[playerID].score.ToString();
    }

    public void Resume()
    {
        Paused = false;
        Time.timeScale = 1;
        UIManager.HideUiElement("Background", "WATWAT", false);
        UIManager.HideUiElement("PauseMenu", "WATWAT", false);
        uiInputModule.RewiredPlayerIds = new int[0];
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    public void TogglePause(int playerID)
    {
        if (Paused && uiInputModule.RewiredPlayerIds[0] == playerID)
        {
            Resume();
        }
        else if (!Paused)
        {
            Pause();
            uiInputModule.RewiredPlayerIds = new int[] { playerID };
        }
    }
}
