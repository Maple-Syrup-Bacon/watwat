using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField]
    private float speed = 10.0f;
    [SerializeField]
    private float maxDistanceToPlayer = 20.0f;
    [SerializeField]
    private float maxHeightDifferenceToPlayer = 2.0f;

    private Vector3 direction;

    private Transform player;
    private Rigidbody body;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        direction = (player.position - gameObject.transform.position);
    }

    private void FixedUpdate()
    {
        if (direction.magnitude < maxDistanceToPlayer && Mathf.Abs(direction.y) <= maxHeightDifferenceToPlayer)
        {
            direction = direction.normalized;
            body.velocity = new Vector3(direction.x * speed * Time.fixedDeltaTime, body.velocity.y);
        }
        else
        {
            body.velocity = Vector3.zero;
        }
    }
}