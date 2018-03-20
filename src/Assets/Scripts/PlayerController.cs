﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using static Utilities;

public class PlayerController : MonoBehaviour
{
    public int playerID;
    public float damageTotal;
    public bool isDead = false;
    public float movementSpeed = 10f;
    public float jumpForce = 10f;
    public float degroundedTime = 0.25f;
    public float maxSpeed = 200f;
    public float slowestTimeFactor = 0.1f;
    public AnimationCurve curve;
    public GameObject lightProjectile;
    public float lightProjectileSpeed = 50f;
    public float lightProjectileCooldown = 1.5f;
    public GameObject punchEffect;
    public float punchDamage = 10f;
    public float punchCooldown = 1f;
    public float timeHitCooldown = 1f;

    public bool IsGrounded { get; set; }
    public PlanetGravity CurrentPlanet { get; set; }

    private Rewired.Player player;
    private GameManager gameManager;
    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private ObjectGravity objectGravity;

    private Vector2 leftAnalogStick;
    private Vector2 aimer;
    private bool jump;
    private float originalFixedDelta;
    private float currentCurveTime = 1.0f;
    private float nextLightProjectile;
    private float nextPunch;
    private float nextTimeHit;
    private float prevX;
    private Vector2 movement;
    private float yExtent;
    private float sqrMaxSpeed;
    private bool attacking;
    private PlayerAttack playerAttack;
    private float attackTriggerXRight;
    private float attackTriggerXLeft;
    private bool isFacingRight = true;
    private bool degrounded = false;
    private float degroundedTimer;

    // Powerups
    public bool HasExplodingFireball { get; set; } = false;
    public bool HasInvincibility { get; set; } = false;
    public bool HasSuperStrength { get; set; } = false;
    public bool HasSuperSpeed { get; set; } = false;

    // Use this for initialization
    private void Start()
    {
        playerAttack = transform.GetChild(0).GetComponent<PlayerAttack>();
        objectGravity = GetComponent<ObjectGravity>();
        yExtent = GetComponent<BoxCollider2D>().bounds.extents.y;
        player = ReInput.players.GetPlayer(playerID);
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalFixedDelta = Time.fixedDeltaTime;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        leftAnalogStick = new Vector2();
        aimer = Vector2.right;
        sqrMaxSpeed = Mathf.Pow(maxSpeed, 2);
        attackTriggerXRight = playerAttack.transform.localPosition.x;
        attackTriggerXLeft = -attackTriggerXRight;
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead)
        {
            return;
        }

        GroundCheck();
        GetInput();
        // DoTime();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        if (movement != Vector2.zero)
        {
            var attackTriggerPos = playerAttack.transform.localPosition;

            playerAttack.transform.localPosition = attackTriggerPos;


            if (IsGrounded)
            {
                // var localVelocity = transform.InverseTransformDirection(body.velocity);
                // localVelocity.x = movement.x * movementSpeed * Time.fixedDeltaTime;

                // body.velocity = transform.TransformDirection(localVelocity);

                var localVelocity = transform.InverseTransformDirection(body.velocity);

                var scaledMovment = (movement - new Vector2(transform.up.x, transform.up.y));

                scaledMovment = (scaledMovment * movementSpeed * Time.fixedDeltaTime);

                localVelocity = transform.InverseTransformDirection(scaledMovment);
                localVelocity.y = 0.0f;

                isFacingRight = (0.0f <= localVelocity.x);

                body.velocity = transform.TransformDirection(localVelocity);
            }
            else
            {
                body.AddForce(movement * movementSpeed * Time.fixedDeltaTime);

                var localMovement = transform.InverseTransformDirection(movement);
                isFacingRight = (0.0f <= localMovement.x);
            }

            if (isFacingRight)
            {
                spriteRenderer.flipX = false;
                attackTriggerPos.x = attackTriggerXRight;
            }
            else
            {
                spriteRenderer.flipX = true;
                attackTriggerPos.x = attackTriggerXLeft;
            }
        }
        else
        {
            // animator.SetLayerWeight(1, 0.0f);

            if (IsGrounded)
            {
                var localVelocity = transform.InverseTransformDirection(body.velocity);
                localVelocity.x = 0.0f;

                body.velocity = transform.TransformDirection(localVelocity);
            }
        }

        if (jump)
        {
            jump = false;
            Deground();
            body.AddForce(transform.up * jumpForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
        }

        if (sqrMaxSpeed < body.velocity.sqrMagnitude)
        {
            // var breakSpeed = body.velocity.magnitude - maxSpeed;
            // var breakVector = body.velocity.normalized * breakSpeed;

            // body.AddForce(-breakVector);

            // body.velocity = body.velocity.normalized * maxSpeed;

            // body.velocity = Vector3.ClampMagnitude(body.velocity, maxSpeed);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Border"))
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        body.velocity = Vector2.zero;

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.respawnTime);
        isDead = false;

        damageTotal = 0;

        var index = Random.Range(0, GameManager.instance.planets.Length);
        transform.position = GameManager.instance.planets[index].transform.position;
    }

    private void GetInput()
    {
        movement.x = player.GetAxis("Move X");
        movement.y = player.GetAxis("Move Y");

        if (!jump && IsGrounded)
        {
            jump = player.GetButtonDown("Jump");
        }

        if (player.GetButtonDown("Primary"))
        {
            if (HasExplodingFireball)
            {
                // Send off a fireball
                // var instance = Instantiate(lightProjectile, transform.position, lightProjectile.transform.rotation);
                // instance.GetComponent<Rigidbody2D>().AddForce(mouseDirection * lightProjectileSpeed, ForceMode2D.Impulse);
                Debug.Log("Shot Fireball!");
            }
        }

        if (player.GetButtonDown("Secondary"))
        {
            Instantiate(punchEffect, transform.position, lightProjectile.transform.rotation);

            foreach (var player in playerAttack.players)
            {
                player.Damage(punchDamage, (isFacingRight ? transform.right : -transform.right));
            }
        }
    }

    private void GroundCheck()
    {
        if (degrounded && degroundedTimer < Time.time)
        {
            degrounded = false;
        }

        if (!degrounded)
        {
            var start = transform.position;
            start -= yExtent * transform.up;

            var end = start;
            end -= transform.up * 0.1f;

            Debug.DrawLine(start, end);
            var ray = Physics2D.Linecast(start, end);

            if (ray.collider != null && ray.collider.CompareTag("Planet"))
            {
                IsGrounded = true;
                transform.parent = ray.collider.transform;
                // CurrentPlanet = ray.collider.gameObject.GetComponent<PlanetGravity>();
            }
            else
            {
                IsGrounded = false;
                transform.parent = null;
                // CurrentPlanet = null;
            }
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

    private void Deground()
    {
        degrounded = true;
        IsGrounded = false;
        degroundedTimer = Time.time + degroundedTime;
    }

    public void Damage(float amount, Vector2 direction)
    {
        damageTotal += amount;
        body.AddForce(direction.normalized * damageTotal, ForceMode2D.Impulse);
    }

    public void SetGrounded(bool grounded)
    {
        this.IsGrounded = grounded;
    }
}
