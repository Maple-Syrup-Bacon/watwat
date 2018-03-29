using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;
using Rewired;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager instance;

    [Header("Spawning")]
    public GameObject powerupPrefab;
    public GameObject[] particlePrefabs;
    public float minSeconds = 10f;
    public float maxSeconds = 20f;

    [Header("Variables")]
    public float invincibilityDuration = 10.0f;
    public float superStrengthDuration = 10.0f;
    public float superSpeedDuration = 10.0f;

    [Header("Sprites")]
    public Sprite[] sprites;

    [Header("Particles")]
    public GameObject hasExplodingFireballParticle;

    private IEnumerator[] activePlayerPowerups;
    private GameObject[] activePlayerPowerupEffects;
    private BoxCollider2D borderBox;


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

    private void Start()
    {
        activePlayerPowerups = new IEnumerator[Utilities.NumberOfPlayers];
        activePlayerPowerupEffects = new GameObject[Utilities.NumberOfPlayers];
        borderBox = GameObject.FindGameObjectWithTag("Border").GetComponent<BoxCollider2D>();

        StartCoroutine(SpawnPowerups());
    }

    public void EnablePowerup(PlayerController player, PowerupType type)
    {
        var playerID = player.playerID;
        try
        {
            StopCoroutine(activePlayerPowerups[playerID]);
        }
        catch (Exception)
        { }
        try
        {
            Destroy(activePlayerPowerupEffects[playerID]);
            activePlayerPowerupEffects[playerID] = null;
        }
        catch (Exception)
        { }
        DisablePowerups(player);
        IEnumerator coroutine;
        switch (type)
        {
            case PowerupType.ExplodingFireball:
                coroutine = ExplodingFireball(player);
                break;
            case PowerupType.Invincibility:
                coroutine = Invincibility(player);
                break;
            case PowerupType.SuperStrength:
                coroutine = SuperStrength(player);
                break;
            case PowerupType.SuperSpeed:
            default:
                coroutine = SuperSpeed(player);
                break;
        }
        StartCoroutine(coroutine);
        activePlayerPowerups[playerID] = coroutine;
    }

    private IEnumerator ExplodingFireball(PlayerController player)
    {
        EnableExplodingFireball(player);
        var playerID = player.playerID;
        var rewiredPlayer = ReInput.players.GetPlayer(playerID);
        while (!rewiredPlayer.GetButtonDown("Primary"))
        {
            yield return null;
        }
        Destroy(activePlayerPowerupEffects[playerID]);
        activePlayerPowerupEffects[playerID] = null;
        DisablePowerups(player);
    }

    private void EnableExplodingFireball(PlayerController player)
    {
        if (!player.HasExplodingFireball)
        {
            player.HasExplodingFireball = true;
            var fire = Instantiate(hasExplodingFireballParticle, Vector3.zero, hasExplodingFireballParticle.transform.rotation, player.transform);
            fire.transform.up = player.transform.up;
            fire.transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            fire.transform.localPosition = new Vector3(0.0f, -player.yExtent, 0.0f);
            activePlayerPowerupEffects[player.playerID] = fire;
        }
    }

    private IEnumerator Invincibility(PlayerController player)
    {
        EnableInvincibility(player);
        yield return new WaitForSeconds(invincibilityDuration);
        DisablePowerups(player);
    }

    private void EnableInvincibility(PlayerController player)
    {
        player.HasInvincibility = true;
    }

    private IEnumerator SuperStrength(PlayerController player)
    {
        EnableSuperStrength(player);
        yield return new WaitForSeconds(superStrengthDuration);
        DisablePowerups(player);
    }

    private void EnableSuperStrength(PlayerController player)
    {
        player.HasSuperStrength = true;
    }

    private IEnumerator SuperSpeed(PlayerController player)
    {
        EnableSuperSpeed(player);
        yield return new WaitForSeconds(superSpeedDuration);
        DisablePowerups(player);
    }

    private void EnableSuperSpeed(PlayerController player)
    {
        player.HasSuperSpeed = true;
    }

    private void DisablePowerups(PlayerController player)
    {
        player.HasExplodingFireball = false;
        player.HasInvincibility = false;
        player.HasSuperStrength = false;
        player.HasSuperSpeed = false;
    }

    private IEnumerator SpawnPowerups()
    {
        while (!GameManager.instance.GameOver)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(minSeconds, maxSeconds));

            var x = UnityEngine.Random.Range(-borderBox.bounds.extents.x, borderBox.bounds.extents.x);
            var y = UnityEngine.Random.Range(-borderBox.bounds.extents.y, borderBox.bounds.extents.y);

            PowerupType type;
            var typeID = UnityEngine.Random.Range(0, 4);

            switch (typeID)
            {
                case 0:
                    type = PowerupType.Invincibility;
                    break;

                case 1:
                    type = PowerupType.SuperStrength;
                    break;

                case 2:
                    type = PowerupType.SuperSpeed;
                    break;

                case 3:
                default:
                    type = PowerupType.ExplodingFireball;
                    break;
            }

            var instance = Instantiate(powerupPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            instance.GetComponent<PowerupController>().type = type;
            instance.GetComponent<SpriteRenderer>().sprite = sprites[typeID];
            var powerupParticles = Instantiate(particlePrefabs[typeID], new Vector3(x, y, 0f), Quaternion.identity, instance.transform);
        }
    }
}
