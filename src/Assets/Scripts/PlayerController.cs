using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private int playerID;

    [SerializeField]
    private float movementSpeed = 10f;

    [SerializeField]
    private float jumpForce = 10f;

    [SerializeField]
    private float maxSpeed = 200f;

    [SerializeField]
    private float slowestTimeFactor = 0.1f;

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
    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 leftAnalogStick;
    private Vector2 aimer;
    private bool jump;
    private bool grounded;
    private float originalFixedDelta;
    private float currentCurveTime = 1.0f;
    private float nextLightProjectile;
    private float nextPunch;
    private float nextTimeHit;
    private float prevX;
    private float movement;
    private float yExtent;

    // Use this for initialization
    private void Start()
    {
        yExtent = GetComponent<BoxCollider2D>().bounds.extents.y;
        player = ReInput.players.GetPlayer(playerID);
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalFixedDelta = Time.fixedDeltaTime;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        leftAnalogStick = new Vector2();
        aimer = Vector2.right;
    }

    // Update is called once per frame
    private void Update()
    {
        GroundCheck();
        GetInput();
        // DoTime();
    }

    private void FixedUpdate()
    {
        if (movement != 0.0f)
        {
            spriteRenderer.flipX = (movement < 0.0f);

            var localVelocity = transform.InverseTransformDirection(body.velocity);
            localVelocity.x = movement * movementSpeed * Time.fixedDeltaTime;

            body.velocity = transform.TransformDirection(localVelocity);
        }
        else
        {
            // animator.SetLayerWeight(1, 0.0f);

            var localVelocity = transform.InverseTransformDirection(body.velocity);
            localVelocity.x = 0.0f;

            body.velocity = transform.TransformDirection(localVelocity);
        }

        if (jump)
        {
            jump = false;
            body.AddRelativeForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
        }

        if (maxSpeed < body.velocity.magnitude)
        {
            body.velocity = body.velocity.normalized * maxSpeed;
        }
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
        movement = player.GetAxis("Move X");

        if (!jump && grounded)
        {
            jump = player.GetButtonDown("Jump");
        }

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

        if (player.GetButtonDown("Primary"))
        {
            var worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(player.controllers.Mouse.screenPosition.x, player.controllers.Mouse.screenPosition.y, transform.position.z));
            var mouseDirection = (worldMousePos - transform.position).normalized;

            var instance = Instantiate(lightProjectile, transform.position, lightProjectile.transform.rotation);
            instance.GetComponent<Rigidbody2D>().AddForce(mouseDirection * lightProjectileSpeed, ForceMode2D.Impulse);
        }

        if (player.GetButtonDown("Secondary"))
        {
            Instantiate(punchEffect, transform.position, lightProjectile.transform.rotation);
        }
    }

    private void GroundCheck()
    {
        var start = transform.position;
        start -= yExtent * transform.up;

        var end = start;
        end -= transform.up;

        Debug.DrawLine(start, end);
        var ray = Physics2D.Linecast(start, end);

        if (ray.collider != null && ray.collider.CompareTag("Planet"))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    private float GetAngle(Vector3 from, Vector3 to)
    {
        var angle = Vector3.Angle(from, to);

        if (from.y < 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }

    private float SnapAngle(float angle)
    {
        return Mathf.RoundToInt(angle / 22.5f) * 22.5f;
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
        // currentCurveTime -= Time.unscaledDeltaTime;
        // currentCurveTime += body.velocity.magnitude * Time.unscaledDeltaTime;
        // currentCurveTime += Mathf.Abs(body.velocity.y) * Time.unscaledDeltaTime;

        // if (jump)
        // {
        //     currentCurveTime = 1f;
        // }

        if (prevX <= leftAnalogStick.x)
        {
            currentCurveTime += Time.unscaledDeltaTime;
            currentCurveTime = Mathf.Clamp(currentCurveTime, 0f, Mathf.Abs(leftAnalogStick.x));
        }
        else
        {
            currentCurveTime -= Time.unscaledDeltaTime;
        }

        // currentCurveTime += body.velocity.sqrMagnitude * Time.unscaledDeltaTime;

        currentCurveTime = Mathf.Clamp(currentCurveTime, 0f, 1f);

        Time.timeScale = Mathf.Clamp(curve.Evaluate(currentCurveTime), slowestTimeFactor, 1f);
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;
    }

    public void SetGrounded(bool grounded)
    {
        this.grounded = grounded;
    }
}
