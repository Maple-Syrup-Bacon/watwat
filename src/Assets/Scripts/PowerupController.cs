using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;

public class PowerupController : MonoBehaviour
{
    public PowerupType type;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (!player.Powerup.HasValue)
            {
                player.Powerup = type;
                Destroy(gameObject);
            }
        }
    }
}
