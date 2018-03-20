using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager instance;

    public float powerupDuration = 10.0f;

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
        players = GameManager.instance.players;
        activePlayerPowerups = new IEnumerator[4];
    }

    public void EnablePowerup(PlayerController player)
    {
        PowerupType? type = player.Powerup;
        if (type.HasValue)
        {
            var playerID = player.playerID;
            try
            {
                StopCoroutine(activePlayerPowerups[playerID]);
            }
            catch (Exception)
            { }
            DisablePowerup(player);
            IEnumerator coroutine;
            switch (type.Value)
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
            player.Powerup = null;
        }
    }

    private IEnumerator ExplodingFireball(PlayerController player)
    {
        EnableExplodingFireball(player);
        yield return new WaitForSeconds(powerupDuration);
        DisablePowerup(player);
    }

    private void EnableExplodingFireball(PlayerController player)
    {
        // Set new variables
    }

    private IEnumerator Invincibility(PlayerController player)
    {
        EnableInvincibility(player);
        yield return new WaitForSeconds(powerupDuration);
        DisablePowerup(player);
    }

    private void EnableInvincibility(PlayerController player)
    {
        // Set new variables
    }

    private IEnumerator SuperStrength(PlayerController player)
    {
        EnableSuperStrength(player);
        yield return new WaitForSeconds(powerupDuration);
        DisablePowerup(player);
    }

    private void EnableSuperStrength(PlayerController player)
    {
        // Set new variables
    }

    private IEnumerator SuperSpeed(PlayerController player)
    {
        EnableSuperSpeed(player);
        yield return new WaitForSeconds(powerupDuration);
        DisablePowerup(player);
    }

    private void EnableSuperSpeed(PlayerController player)
    {
        // Set new variables
    }

    private void DisablePowerup(PlayerController player)
    {
        // Set default variables
    }
}
