using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;
using Rewired;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager instance;

    public float invincibilityDuration = 10.0f;
    public float superStrengthDuration = 10.0f;
    public float superSpeedDuration = 10.0f;

    private PlayerController[] players;
    private IEnumerator[] activePlayerPowerups;

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
        players = GameManager.instance.Players;
        activePlayerPowerups = new IEnumerator[4];
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
        var rewiredPlayer = ReInput.players.GetPlayer(player.playerID);
        while (!rewiredPlayer.GetButtonDown("Primary"))
        {
            yield return null;
        }
        DisablePowerups(player);
    }

    private void EnableExplodingFireball(PlayerController player)
    {
        player.HasExplodingFireball = true;
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
}
