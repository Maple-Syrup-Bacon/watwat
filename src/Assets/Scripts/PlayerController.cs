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
    [SerializeField]
    private float slowestTimeFactor = 0.1f;
    [SerializeField]
    private float speedUpFactor = 0.1f;
    [SerializeField]
    private float speedUpExponent = 2f;
    [SerializeField]
    private float slowDownFactor = 0.1f;
    [SerializeField]
    private float slowDownExponent = 2f;
    [SerializeField]
    private float magnitudeFactor = 4f;
    [SerializeField]
    private AnimationCurve curve;

    private Rewired.Player player;

    private Rigidbody body;
    private float movement;
    private bool jump;
    private bool grounded;
    private float originalFixedDelta;
    private float currentCurveTime = 1.0f;

    // Use this for initialization
    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
        body = GetComponent<Rigidbody>();
        originalFixedDelta = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    private void Update()
    {
        GetInput();
        DoTime();
    }

    private void FixedUpdate()
    {
        body.velocity = new Vector3(movement * movementSpeed * Time.fixedDeltaTime, body.velocity.y, 0.0f);

        if (jump)
        {
            jump = false;
            body.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    private void GetInput()
    {
        movement = player.GetAxis("Move");

        if (!jump && grounded)
        {
            jump = player.GetButtonDown("Jump");
        }
    }

    private void DoTime()
    {
        // if (movement != 0.0f)
        // {
        //     Time.timeScale += Mathf.Pow(speedUpFactor * Time.unscaledDeltaTime, speedUpExponent);
        // }
        // else
        // {
        //     Time.timeScale -= Mathf.Pow(slowDownFactor * Time.unscaledDeltaTime, slowDownExponent);
        // }

        // if (movement != 0.0f)
        // {
        //     Time.timeScale = Mathf.Lerp(Time.timeScale, Mathf.Abs(movement), Time.unscaledDeltaTime);
        // }
        // else
        // {
        //     Time.timeScale = Mathf.Lerp(Time.timeScale, slowestTimeFactor, Time.unscaledDeltaTime);
        // }

        // float factor = body.velocity.magnitude / magnitudeFactor;
        // factor = Mathf.Clamp(factor, 0f, 1f);

        // if (factor != 0.0f)
        // {
        //     Time.timeScale += Mathf.Pow(factor * speedUpFactor * Time.unscaledDeltaTime, speedUpExponent);
        // }
        // else
        // {
        //     Time.timeScale -= Mathf.Pow(slowDownFactor * Time.unscaledDeltaTime, slowDownExponent);
        // }

        // if (movement != 0.0f)
        // {
        //     Time.timeScale = Mathf.Lerp(Time.timeScale, Mathf.Abs(movement), Time.unscaledDeltaTime);
        // }
        // else
        // {
        //     Time.timeScale = Mathf.Lerp(Time.timeScale, slowestTimeFactor, Time.unscaledDeltaTime);
        // }

        if (body.velocity.magnitude != 0f)
        {
            currentCurveTime += Time.unscaledDeltaTime;
        }
        else
        {
            currentCurveTime -= Time.unscaledDeltaTime;
        }

        currentCurveTime = Mathf.Clamp(currentCurveTime, 0f, 1f);

        // Debug.Log("Current Time: " + currentCurveTime);

        Time.timeScale = Mathf.Clamp(curve.Evaluate(currentCurveTime), slowestTimeFactor, 1f);
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;
    }

    public void SetGrounded(bool grounded)
    {
        this.grounded = grounded;
    }
}
