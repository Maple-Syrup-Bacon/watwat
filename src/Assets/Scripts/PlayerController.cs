using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float movementSpeed = 1000f;
    [SerializeField]
    private float jumpForce = 10f;

    private Rewired.Player player;

    private Rigidbody body;
    private float movement;
    private bool jump;
    private bool grounded;

    // Use this for initialization
    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        movement = 0.0f;

        if (player.GetButton("Move Left"))
        {
            movement += -1.0f;
        }
        if (player.GetButton("Move Right"))
        {
            movement += 1.0f;
        }

        if (!jump && grounded)
        {
            jump = player.GetButton("Jump");
        }
    }

    private void FixedUpdate()
    {
        if (movement != 0.0f)
        {
            body.velocity = new Vector3(movement * movementSpeed * Time.fixedDeltaTime, body.velocity.y, 0.0f);
        }
        else
        {
            body.velocity = new Vector3(0.0f, body.velocity.y, 0.0f);
        }

        if (jump)
        {
            jump = false;
            body.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    public void SetGrounded(bool grounded)
    {
        this.grounded = grounded;
    }
}
