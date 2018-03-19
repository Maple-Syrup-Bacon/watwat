using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public List<PlayerController> players;

    private void Start()
    {
        players = new List<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger && other.CompareTag("Player"))
        {
            players.Add(other.GetComponent<PlayerController>());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.isTrigger && other.CompareTag("Player"))
        {
            players.Remove(other.GetComponent<PlayerController>());
        }
    }
}
