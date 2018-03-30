﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using EazyTools.SoundManager;
using static Utilities;

public class PlayerController : MonoBehaviour
{
    public int playerID;
    public float damageTotal;
    public int score;
    public bool isDead = false;
    public float movementSpeed = 10f;
    public float jumpForce = 10f;
    public float degroundedTime = 0.25f;
    public float maxSpeed = 200f;
    public float lightProjectileSpeed = 50f;
    public float lightProjectileCooldown = 1.5f;
    public float meleeDamage = 10f;
    public float meleeCooldown = 1f;
    public float invincibilityFrameTime = 0.1f;
    public float yExtent;

    [Header("Dash")]
    public float dashForce = 100f;
    public float dashDuration = 1f;
    public float dashDrag = 1f;

    [Header("Powerup Effects")]
    public float superStrengthEffect = 4.0f;
    public float superSpeedEffect = 2.0f;

    [Header("Particles")]
    public GameObject meleeHitParticle;
    public GameObject meleeParticle;
    public GameObject deathParticle;
    public GameObject spawnParticle;
    public GameObject lightProjectile;

    [Header("Camera Shake")]
    public float deathMagnitude = 100;
    public float deathRoughness = 10;
    public float deathFadeIn = 0.25f;
    public float deathFadeOut = 1;


    [Header("Audio")]
    public AudioClip meleeHit;
    public float meleeHitVolume = 1.0f;

    // Properties
    public bool IsGrounded { get; set; }
    public bool IsVisible { get; set; }
    public PlanetGravity CurrentPlanet { get; set; }
    public Sprite IdleSprite { get; set; }
    public RuntimeAnimatorController AnimatorController { get; set; }

    private Rewired.Player player;
    private GameManager gameManager;
    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private ObjectGravity objectGravity;
    private Transform aimerTransform;
    private SpriteRenderer aimerSpriteRenderer;
    private TrailRenderer trailRenderer;
    private ParticleSystem.MainModule meleeParticlePrefabMain;

    private Vector3 aimerVector;
    private Vector2 leftAnalogStick;
    private bool jump;
    private float originalFixedDelta;
    private float currentCurveTime = 1.0f;
    private float nextLightProjectile;
    private float nextMelee;
    private float nextTimeHit;
    private float prevX;
    private Vector2 movement;
    private float sqrMaxSpeed;
    private bool attacking;
    private PlayerAttack playerAttack;
    private float attackTriggerXRight;
    private float attackTriggerXLeft;
    private bool isFacingRight = true;
    private bool degrounded = false;
    private float degroundedTimer;
    private bool inInvincibilityFrame = false;
    private float invincibilityFrameTimer;
    private bool dashDisabled = false;
    private int lastDamagedByPlayerID = -1;

    // Powerups
    public bool HasFireball { get; set; } = false;
    public bool HasInvincibility { get; set; } = false;
    public bool HasSuperStrength { get; set; } = false;
    public bool HasSuperSpeed { get; set; } = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    private void Start()
    {
        aimerTransform = transform.GetChild(0);
        playerAttack = aimerTransform.GetComponent<PlayerAttack>();
        aimerSpriteRenderer = aimerTransform.GetComponent<SpriteRenderer>();
        meleeParticlePrefabMain = meleeParticle.GetComponent<ParticleSystem>().main;
        trailRenderer = GetComponent<TrailRenderer>();
        yExtent = GetComponent<BoxCollider2D>().bounds.extents.y;
        player = ReInput.players.GetPlayer(playerID);
        originalFixedDelta = Time.fixedDeltaTime;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        leftAnalogStick = new Vector2();
        sqrMaxSpeed = Mathf.Pow(maxSpeed, 2);
        attackTriggerXRight = playerAttack.transform.localPosition.x;
        attackTriggerXLeft = -attackTriggerXRight;
        aimerVector = Vector2.right;
        IsVisible = true;
        animator.runtimeAnimatorController = AnimatorController;
        spriteRenderer.sprite = IdleSprite;
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead)
        {
            return;
        }

        GroundCheck();

        if (!GameManager.instance.GameStarted || GameManager.instance.GameOver)
        {
            return;
        }

        if (inInvincibilityFrame && invincibilityFrameTimer <= Time.time)
        {
            inInvincibilityFrame = false;
        }

        GetInput();
        MoveAimer();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        animator.enabled = true;

        if (movement != Vector2.zero)
        {
            var attackTriggerPos = playerAttack.transform.localPosition;

            playerAttack.transform.localPosition = attackTriggerPos;

            if (IsGrounded)
            {
                animator.SetLayerWeight(1, 1.0f);

                var localVelocity = transform.InverseTransformDirection(body.velocity);

                var scaledMovement = (movement - new Vector2(transform.up.x, transform.up.y));

                scaledMovement = (scaledMovement * movementSpeed * Time.fixedDeltaTime);

                if (HasSuperSpeed)
                {
                    scaledMovement *= superSpeedEffect;
                }

                localVelocity = transform.InverseTransformDirection(scaledMovement);
                localVelocity.y = 0.0f;

                isFacingRight = (0.0f <= localVelocity.x);

                body.velocity = transform.TransformDirection(localVelocity);
            }
            else
            {
                var newMovement = movement * movementSpeed * Time.fixedDeltaTime;
                if (HasSuperSpeed)
                {
                    newMovement *= superSpeedEffect;
                }
                body.AddForce(newMovement);

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
            animator.SetLayerWeight(1, 0.0f);

            if (IsGrounded)
            {
                animator.enabled = false;
                spriteRenderer.sprite = IdleSprite;

                var localVelocity = transform.InverseTransformDirection(body.velocity);
                localVelocity.x = 0.0f;

                body.velocity = transform.TransformDirection(localVelocity);
            }
        }

        if (!IsGrounded)
        {
            animator.SetLayerWeight(2, 1.0f);
        }
        else
        {
            animator.SetLayerWeight(2, 0.0f);
        }

        if (jump)
        {
            jump = false;

            var superSpeedBonus = (HasSuperSpeed ? superSpeedEffect : 1f);

            if (IsGrounded)
            {
                Deground();
                var jumpVec = movement == Vector2.zero ? new Vector2(transform.up.x, transform.up.y) : movement.normalized;
                body.AddForce(jumpVec * jumpForce * superSpeedBonus * Time.fixedDeltaTime, ForceMode2D.Impulse);
            }
            else if (!dashDisabled)
            {
                dashDisabled = !HasSuperSpeed;

                var dashVec = body.velocity;
                var bonusForce = 0f;
                if (movement != Vector2.zero)
                {
                    var velocityAngle = Mathf.Abs(Vector2.SignedAngle(movement, body.velocity));
                    dashVec = movement;

                    if (velocityAngle < 90)
                    {
                        bonusForce = 1 - (velocityAngle / 90);
                    }
                }
                body.velocity *= bonusForce;
                body.AddForce(dashVec.normalized * dashForce * superSpeedBonus * Time.fixedDeltaTime, ForceMode2D.Impulse);
                StartCoroutine(DashStop());
            }
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
        if (!isDead && other.CompareTag("Border"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.instance.GameOver)
        {
            return;
        }

        isDead = true;
        body.velocity = Vector2.zero;
        Instantiate(deathParticle, transform.position, deathParticle.transform.rotation);

        if (lastDamagedByPlayerID != -1)
        {
            GameManager.instance.IncreaseScore(lastDamagedByPlayerID);
            lastDamagedByPlayerID = -1;
        }
        else
        {
            score--;
        }

        GameManager.instance.UpdateAvatars();

        EZCameraShake.CameraShaker.Instance.ShakeOnce(deathMagnitude, deathRoughness, deathFadeIn, deathFadeOut);
        PowerupManager.instance.DisablePowerups(this);

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        spriteRenderer.enabled = false;
        aimerSpriteRenderer.enabled = false;
        trailRenderer.enabled = false;

        yield return new WaitForSeconds(GameManager.instance.timeVisibleAfterDeath);

        IsVisible = false;

        yield return new WaitForSeconds(GameManager.instance.respawnTime - GameManager.instance.timeVisibleAfterDeath);

        isDead = false;
        IsVisible = true;
        spriteRenderer.enabled = true;
        aimerSpriteRenderer.enabled = true;
        trailRenderer.enabled = true;

        damageTotal = 0;

        var index = Random.Range(0, GameManager.instance.Planets.Length);
        transform.position = GameManager.instance.Planets[index].transform.position;

        yield return new WaitForSeconds(GameManager.instance.respawnParticleDelay);

        var instance = Instantiate(spawnParticle, transform.position, spawnParticle.transform.rotation);

        instance.transform.up = transform.up;
        instance.transform.Rotate(new Vector3(90, 0, 0));

        var pos = instance.transform.localPosition;
        pos.y -= yExtent;
        instance.transform.localPosition = pos;

        GameManager.instance.UpdateAvatars();
    }

    private void GetInput()
    {
        movement.x = player.GetAxis("Move X");
        movement.y = player.GetAxis("Move Y");

        if (movement != Vector2.zero)
        {
            aimerVector = movement.normalized;
        }

        if (!jump)
        {
            jump = player.GetButtonDown("Jump");
        }

        if (player.GetButtonDown("Primary") && nextMelee < Time.time)
        {
            nextMelee = Time.time + meleeCooldown;

            if (HasFireball)
            {
                var instance = Instantiate(lightProjectile, aimerTransform.position, lightProjectile.transform.rotation);
                instance.GetComponent<BasicProjectile>().Owner = transform;
                instance.GetComponent<Rigidbody2D>().AddForce(aimerVector * lightProjectileSpeed, ForceMode2D.Impulse);
            }
            else
            {
                var rot = -(aimerTransform.rotation.eulerAngles.z + 90);
                meleeParticlePrefabMain.startRotation = new ParticleSystem.MinMaxCurve(rot * Mathf.Deg2Rad);

                var meleeParticeInstance = Instantiate(meleeParticle, aimerTransform.position, Quaternion.identity);

                var damage = (HasSuperStrength ? meleeDamage * superStrengthEffect : meleeDamage);

                foreach (var player in playerAttack.players)
                {
                    player.Damage(damage, player.transform.position - transform.position, playerID, body.velocity);
                    SoundManager.PlaySound(meleeHit, meleeHitVolume);
                }
            }
        }

        if (player.GetButtonDown("Secondary"))
        { }
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
                dashDisabled = false;
                transform.parent = ray.collider.transform;
            }
            else
            {
                IsGrounded = false;
                transform.parent = null;
            }
        }
    }

    private void MoveAimer()
    {
        var aimerAngle = Mathf.Atan2(aimerVector.y, aimerVector.x) * Mathf.Rad2Deg;
        aimerAngle -= 90f;

        aimerTransform.position = transform.position + aimerVector;
        aimerTransform.rotation = Quaternion.Euler(0, 0, aimerAngle);
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

    private void Deground()
    {
        degrounded = true;
        IsGrounded = false;
        degroundedTimer = Time.time + degroundedTime;
    }

    private IEnumerator InvincibilityFrameBlink()
    {
        var interval = invincibilityFrameTime / 2;
        var originalColor = spriteRenderer.color;

        while (inInvincibilityFrame)
        {
            var color = spriteRenderer.color;
            color.a = 0.5f;
            spriteRenderer.color = color;

            yield return new WaitForSeconds(interval);

            color.a = 1f;
            spriteRenderer.color = color;
        }

        spriteRenderer.color = originalColor;
    }

    private IEnumerator DashStop()
    {
        var elapsed = 0f;
        while (!IsGrounded && elapsed < dashDuration)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            elapsed += Time.deltaTime;
            body.drag += dashDrag;
        }

        body.drag = 0f;
    }

    public void Damage(float amount, Vector2 direction, int enemyPlayerID, Vector2? velocity = null)
    {
        if (inInvincibilityFrame)
        {
            return;
        }

        inInvincibilityFrame = true;
        invincibilityFrameTimer = invincibilityFrameTime + Time.time;
        StartCoroutine(InvincibilityFrameBlink());

        lastDamagedByPlayerID = enemyPlayerID;

        amount = (HasInvincibility ? amount / 2 : amount);

        damageTotal += Mathf.Round(amount);
        var hitParticle = Instantiate(meleeHitParticle, transform.position, meleeHitParticle.transform.rotation);

        var bonusVec = Vector2.zero;
        if (velocity.HasValue)
        {
            bonusVec = velocity.Value;
            damageTotal += Mathf.Round(bonusVec.magnitude / 2);
        }

        var knockbackForce = (GameManager.instance.baseKnockback * (damageTotal / 100f)) + (bonusVec.magnitude / 2);
        hitParticle.transform.localScale *= knockbackForce / 10;

        var knockbackVec = bonusVec.normalized + direction.normalized;

        knockbackForce = (HasInvincibility ? knockbackForce / 2 : knockbackForce);

        body.AddForce(knockbackVec.normalized * knockbackForce, ForceMode2D.Impulse);
        Deground();

        GameManager.instance.UpdateAvatars();
    }

    public void SetGrounded(bool grounded)
    {
        this.IsGrounded = grounded;
    }
}
