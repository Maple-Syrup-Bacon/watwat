﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyTools.SoundManager;
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
    public float superStrengthSizeModifier = 1.2f;

    [Header("Sprites")]
    public Sprite[] sprites;

    [Header("Particles")]
    public GameObject hasExplodingFireballParticle;
    public GameObject hasSuperSpeedParticle;
    public GameObject hasSuperStrengthParticle;
    public GameObject hasInvincibilityParticle;
    public GameObject fireballClubTorch;

    [Header("Colors")]
    public Color fireballColor;
    public Color superSpeedColor;
    public Color superStrengthColor;
    public Color invincibilityColor;

    [Header("Audio")]
    public AudioClip hasFireballSound;
    public float hasFireballVolume = 1.0f;
    public AudioClip hasSuperSpeedSound;
    public float hasSuperSpeedVolume = 1.0f;
    public AudioClip hasSuperStrengthSound;
    public AudioClip hasInvincibilitySound;

    private float[] superSpeedTimers;
    private float[] superStrengthTimers;
    private float[] invincibilityTimers;

    private GameObject[] fireballEffects;
    private GameObject[] torchFireballEffects;
    private GameObject[] superSpeedEffects;
    private GameObject[] superStrengthEffects;
    private GameObject[] invincibilitiesEffects;

    private Audio[] hasFireballAudios;
    private Audio[] hasSuperSpeedAudios;
    private Audio[] hasSuperStrengthAudios;
    private Audio[] hasInvincibilityAudios;

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
        borderBox = GameObject.FindGameObjectWithTag("Border").GetComponent<BoxCollider2D>();

        superSpeedTimers = new float[Utilities.NumberOfPlayers];
        superStrengthTimers = new float[Utilities.NumberOfPlayers];
        invincibilityTimers = new float[Utilities.NumberOfPlayers];

        fireballEffects = new GameObject[Utilities.NumberOfPlayers];
        torchFireballEffects = new GameObject[Utilities.NumberOfPlayers];
        superSpeedEffects = new GameObject[Utilities.NumberOfPlayers];
        superStrengthEffects = new GameObject[Utilities.NumberOfPlayers];
        invincibilitiesEffects = new GameObject[Utilities.NumberOfPlayers];

        hasFireballAudios = new Audio[Utilities.NumberOfPlayers];
        hasSuperSpeedAudios = new Audio[Utilities.NumberOfPlayers];
        hasSuperStrengthAudios = new Audio[Utilities.NumberOfPlayers];
        hasInvincibilityAudios = new Audio[Utilities.NumberOfPlayers];

        StartCoroutine(SpawnPowerups());
    }

    private void Update()
    {
        foreach (var player in GameManager.instance.Players)
        {
            if (player.HasSuperSpeed && superSpeedTimers[player.playerID] <= Time.time)
            {
                DisableSuperSpeed(player);
            }

            if (player.HasSuperStrength && superStrengthTimers[player.playerID] <= Time.time)
            {
                DisableSuperStrength(player);
            }

            if (player.HasInvincibility && invincibilityTimers[player.playerID] <= Time.time)
            {
                DisableInvincibility(player);
            }
        }
    }

    public void EnablePowerup(PlayerController player, PowerupType type)
    {
        var playerID = player.playerID;
        switch (type)
        {
            case PowerupType.ExplodingFireball:
                EnableFireball(player);
                break;

            case PowerupType.Invincibility:
                Invincibility(player);
                break;

            case PowerupType.SuperStrength:
                SuperStrength(player);
                break;

            case PowerupType.SuperSpeed:
            default:
                SuperSpeed(player);
                break;
        }
    }

    private void EnableFireball(PlayerController player)
    {
        if (!player.HasFireball)
        {
            player.HasFireball = true;
            player.FireballSpriteEffect.enabled = true;
            // GameManager.instance.PlayerLights[player.playerID].color = fireballColor;
            var effect = Instantiate(hasExplodingFireballParticle, Vector3.zero, hasExplodingFireballParticle.transform.rotation, player.transform);
            effect.transform.up = player.transform.up;
            effect.transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            effect.transform.localPosition = new Vector3(0.0f, -player.GetYExtend(), 0.0f);
            fireballEffects[player.playerID] = effect;

            var torchParent = player.transform.Find("WeaponJoint").Find("WeaponIdle");
            var torchEffect = Instantiate(fireballClubTorch, torchParent.transform.position, fireballClubTorch.transform.rotation, torchParent);
            torchEffect.transform.localPosition = new Vector3(-0.7f, 1.54f, 0f);
            torchEffect.transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            torchFireballEffects[player.playerID] = torchEffect;
        }
    }

    private void Invincibility(PlayerController player)
    {
        EnableInvincibility(player);
        invincibilityTimers[player.playerID] = invincibilityDuration + Time.time;
    }

    private void EnableInvincibility(PlayerController player)
    {
        if (!player.HasInvincibility)
        {
            player.HasInvincibility = true;
            // GameManager.instance.PlayerLights[player.playerID].color = invincibilityColor;
            var effect = Instantiate(hasInvincibilityParticle, Vector3.zero, hasInvincibilityParticle.transform.rotation, player.transform);
            effect.transform.up = player.transform.up;
            effect.transform.localPosition = Vector3.zero;
            invincibilitiesEffects[player.playerID] = effect;
        }
    }

    private void SuperStrength(PlayerController player)
    {
        EnableSuperStrength(player);
        superStrengthTimers[player.playerID] = superStrengthDuration + Time.time;
    }

    private void EnableSuperStrength(PlayerController player)
    {
        if (!player.HasSuperStrength)
        {
            player.HasSuperStrength = true;
            player.SetScale(player.transform.localScale * superStrengthSizeModifier);
            // GameManager.instance.PlayerLights[player.playerID].color = superStrengthColor;
        }
    }

    private void SuperSpeed(PlayerController player)
    {
        EnableSuperSpeed(player);
        superSpeedTimers[player.playerID] = superSpeedDuration + Time.time;
    }

    private void EnableSuperSpeed(PlayerController player)
    {
        if (!player.HasSuperSpeed)
        {
            player.HasSuperSpeed = true;
            player.dashDisabled = false;
            player.SuperSpeedSpriteEffect.enabled = true;
            // GameManager.instance.PlayerLights[player.playerID].color = superSpeedColor;
            var effect = Instantiate(hasSuperSpeedParticle, Vector3.zero, hasSuperSpeedParticle.transform.rotation, player.transform);
            effect.transform.up = player.transform.up;
            effect.transform.localPosition = Vector3.zero;
            superSpeedEffects[player.playerID] = effect;
            var audioID = SoundManager.PlaySound(hasSuperSpeedSound, hasSuperSpeedVolume, true, null);
            hasSuperSpeedAudios[player.playerID] = SoundManager.GetAudio(audioID);
        }
    }

    private IEnumerator SpawnPowerups()
    {
        while (!GameManager.instance.GameOver)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minSeconds, maxSeconds));

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

    private void DisableSuperSpeed(PlayerController player)
    {
        player.HasSuperSpeed = false;
        player.SuperSpeedSpriteEffect.enabled = false;
        ResetPlayerLightColor(player.playerID);
        Destroy(superSpeedEffects[player.playerID]);
        superSpeedEffects[player.playerID] = null;
        hasSuperSpeedAudios[player.playerID].Stop();
    }

    private void DisableSuperStrength(PlayerController player)
    {
        player.HasSuperStrength = false;
        player.SetScale(Vector3.one);
        ResetPlayerLightColor(player.playerID);
        Destroy(superStrengthEffects[player.playerID]);
        superStrengthEffects[player.playerID] = null;
    }

    public void DisableFireball(PlayerController player)
    {
        player.HasFireball = false;
        player.FireballSpriteEffect.enabled = false;
        ResetPlayerLightColor(player.playerID);
        Destroy(fireballEffects[player.playerID]);
        fireballEffects[player.playerID] = null;
        Destroy(torchFireballEffects[player.playerID]);
        torchFireballEffects[player.playerID] = null;
    }

    private void DisableInvincibility(PlayerController player)
    {
        player.HasInvincibility = false;
        ResetPlayerLightColor(player.playerID);
        Destroy(invincibilitiesEffects[player.playerID]);
        invincibilitiesEffects[player.playerID] = null;
    }

    private void ResetPlayerLightColor(int playerID)
    {
        GameManager.instance.PlayerLights[playerID].color = Color.white;
    }

    public void DisablePowerups(PlayerController player)
    {
        if (player.HasSuperSpeed)
        {
            DisableSuperSpeed(player);
        }

        if (player.HasSuperStrength)
        {
            DisableSuperStrength(player);
        }

        if (player.HasFireball)
        {
            DisableFireball(player);
        }

        if (player.HasInvincibility)
        {
            DisableInvincibility(player);
        }
    }
}
