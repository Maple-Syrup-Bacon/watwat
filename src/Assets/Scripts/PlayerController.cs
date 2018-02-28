using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed = 10f;

    private Rigidbody body;
    private Vector2 movement;

    // Use this for initialization
    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        movement = Vector2.zero;
        movement.x = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (movement.x != 0.0f)
        {
            body.velocity = new Vector3(movement.x * speed * Time.fixedDeltaTime, body.velocity.y, 0.0f);
            Debug.Log(body.velocity);
        }
        else
        {
            body.velocity = new Vector3(0.0f, body.velocity.y, 0.0f);
        }
    }
}
