using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{

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

    [SerializeField]
    private GameObject lightProjectile;

    [SerializeField]
    private float lightProjectileSpeed = 50f;

    [SerializeField]
    private float lightProjectileCooldown = 1.5f;

    [SerializeField]
    private GameObject punchEffect;

    [SerializeField]
    private float punchCooldown = 1f;

    [SerializeField]
    private float timeHitCooldown = 1f;

    private Rewired.Player player;
    private GameManager gameManager;

    private Rigidbody body;
    private Vector2 leftAnalogStick;
    private Vector2 aimer;
    private bool jump;
    private bool grounded;
    private float originalFixedDelta;
    private float currentCurveTime = 1.0f;
    private float nextLightProjectile;
    private float nextPunch;
    private float nextTimeHit;

    // Use this for initialization
    private void Start()
    {
        player = ReInput.players.GetPlayer(0);
        body = GetComponent<Rigidbody>();
        originalFixedDelta = Time.fixedDeltaTime;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        leftAnalogStick = new Vector2();
        aimer = Vector2.right;
    }

    // Update is called once per frame
    private void Update()
    {
        GetInput();
        DoTime();
    }

    private void FixedUpdate()
    {
        // body.velocity = new Vector3(movement * movementSpeed * Time.fixedDeltaTime, body.velocity.y, 0.0f);

        // if (jump)
        // {
        //     jump = false;
        //     body.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
        // }

        // transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        body.MovePosition(new Vector3(body.position.x, body.position.y, 0f));
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") && nextTimeHit < Time.time)
        {
            nextTimeHit = Time.time + timeHitCooldown;

            gameManager.SubtractTime(5f);
        }
    }

    private void GetInput()
    {
        leftAnalogStick.x = player.GetAxis("Move X");
        leftAnalogStick.y = player.GetAxis("Move Y");

        if (leftAnalogStick != Vector2.zero)
        {
            aimer = leftAnalogStick;
        }

        // if (!jump && grounded)
        // {
        //     jump = player.GetButtonDown("Jump");
        // }

        if (player.GetButtonDown("Light Attack") && nextLightProjectile < Time.time)
        {
            nextLightProjectile = Time.time + lightProjectileCooldown;

            var instance = Instantiate(lightProjectile, transform.position + Vector3.up + (Vector3.forward * (-0.5f)), lightProjectile.transform.rotation);
            instance.GetComponent<Rigidbody>().AddForce(aimer.normalized * lightProjectileSpeed, ForceMode.Impulse);
        }

        if (player.GetButtonDown("Punch") && nextPunch < Time.time)
        {
            nextPunch = Time.time + punchCooldown;

            Instantiate(punchEffect, transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.transform.position + Vector3.up + (Vector3.forward * (-0.5f)), punchEffect.transform.rotation);
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

        // if (leftAnalogStick.x != 0.0f)
        // {
        //     currentCurveTime = Mathf.Abs(leftAnalogStick.x);
        // }
        // else
        // {
        //     currentCurveTime -= Time.unscaledDeltaTime;
        // }

        // currentCurveTime = Mathf.Abs(leftAnalogStick.x) * Time.unscaledDeltaTime;
        currentCurveTime -= Time.unscaledDeltaTime;
        currentCurveTime += body.velocity.magnitude * Time.unscaledDeltaTime;
        // currentCurveTime += Mathf.Abs(body.velocity.y) * Time.unscaledDeltaTime;

        // if (jump)
        // {
        //     currentCurveTime = 1f;
        // }

        currentCurveTime = Mathf.Clamp(currentCurveTime, 0f, 1f);

        Time.timeScale = Mathf.Clamp(curve.Evaluate(currentCurveTime), slowestTimeFactor, 1f);
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;
    }

    public void SetGrounded(bool grounded)
    {
        this.grounded = grounded;
    }
}
