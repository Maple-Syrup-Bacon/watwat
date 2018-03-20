using System.Collections;
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
    public float lightProjectileSpeed = 50f;
    public float lightProjectileCooldown = 1.5f;
    public float meleeDamage = 10f;
    public float meleeCooldown = 1f;
    public float timeHitCooldown = 1f;
    [Header("Particles")]
    public GameObject meleeHitParticle;
    public GameObject meleeParticle;
    public GameObject deathParticle;
    public GameObject spawnParticle;
    public GameObject lightProjectile;

    public bool IsGrounded { get; set; }
    public bool IsVisible { get; set; }
    public PowerupType? Powerup { get; set; }
    public PlanetGravity CurrentPlanet { get; set; }

    private Rewired.Player player;
    private GameManager gameManager;
    private Rigidbody2D body;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private ObjectGravity objectGravity;
    private Transform aimerTransform;
    private SpriteRenderer aimerSpriteRenderer;

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
    private float yExtent;
    private float sqrMaxSpeed;
    private bool attacking;
    private PlayerAttack playerAttack;
    private float attackTriggerXRight;
    private float attackTriggerXLeft;
    private bool isFacingRight = true;
    private bool degrounded = false;
    private float degroundedTimer;

    // Use this for initialization
    private void Start()
    {
        aimerTransform = transform.GetChild(0);
        playerAttack = aimerTransform.GetComponent<PlayerAttack>();
        aimerSpriteRenderer = aimerTransform.GetComponent<SpriteRenderer>();
        yExtent = GetComponent<BoxCollider2D>().bounds.extents.y;
        player = ReInput.players.GetPlayer(playerID);
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalFixedDelta = Time.fixedDeltaTime;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        leftAnalogStick = new Vector2();
        sqrMaxSpeed = Mathf.Pow(maxSpeed, 2);
        attackTriggerXRight = playerAttack.transform.localPosition.x;
        attackTriggerXLeft = -attackTriggerXRight;
        aimerVector = Vector2.right;
        IsVisible = true;
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
        MoveAimer();
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
        if (!isDead && other.CompareTag("Border"))
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        body.velocity = Vector2.zero;
        Instantiate(deathParticle, transform.position, deathParticle.transform.rotation);

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        spriteRenderer.enabled = false;
        aimerSpriteRenderer.enabled = false;

        yield return new WaitForSeconds(GameManager.instance.timeVisibleAfterDeath);

        IsVisible = false;

        yield return new WaitForSeconds(GameManager.instance.respawnTime - GameManager.instance.timeVisibleAfterDeath);

        isDead = false;
        IsVisible = true;
        spriteRenderer.enabled = true;
        aimerSpriteRenderer.enabled = true;

        damageTotal = 0;

        var index = Random.Range(0, GameManager.instance.planets.Length);
        transform.position = GameManager.instance.planets[index].transform.position;

        yield return new WaitForSeconds(GameManager.instance.respawnParticleDelay);

        var instance = Instantiate(spawnParticle, transform.position, spawnParticle.transform.rotation);

        instance.transform.up = transform.up;
        instance.transform.Rotate(new Vector3(90, 0, 0));

        var pos = instance.transform.localPosition;
        pos.y -= yExtent;
        instance.transform.localPosition = pos;
    }

    private void GetInput()
    {
        movement.x = player.GetAxis("Move X");
        movement.y = player.GetAxis("Move Y");

        if (movement != Vector2.zero)
        {
            aimerVector = movement.normalized;
        }

        if (!jump && IsGrounded)
        {
            jump = player.GetButtonDown("Jump");
        }

        if (player.GetButtonDown("Primary"))
        {
            Powerup = PowerupType.ExplodingFireball;
            PowerupManager.instance.EnablePowerup(this);
            // var worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(player.controllers.Mouse.screenPosition.x, player.controllers.Mouse.screenPosition.y, transform.position.z));
            // var mouseDirection = (worldMousePos - transform.position).normalized;

            // var instance = Instantiate(lightProjectile, transform.position, lightProjectile.transform.rotation);
            // instance.GetComponent<Rigidbody2D>().AddForce(mouseDirection * lightProjectileSpeed, ForceMode2D.Impulse);
        }

        if (player.GetButtonDown("Secondary"))
        {
            // Instantiate(meleeParticle, transform.position, lightProjectile.transform.rotation);

            foreach (var player in playerAttack.players)
            {
                player.Damage(meleeDamage, (isFacingRight ? transform.right : -transform.right));
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

    private void MoveAimer()
    {
        var angle = Mathf.Atan2(aimerVector.y, aimerVector.x) * Mathf.Rad2Deg;
        angle -= 90f;

        aimerTransform.position = transform.position + aimerVector;
        aimerTransform.rotation = Quaternion.Euler(0, 0, angle);
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

    public void Damage(float amount, Vector2 direction)
    {
        damageTotal += amount;
        Instantiate(meleeHitParticle, transform.position, meleeHitParticle.transform.rotation);
        body.AddForce(direction.normalized * damageTotal, ForceMode2D.Impulse);
        Deground();
    }

    public void SetGrounded(bool grounded)
    {
        this.IsGrounded = grounded;
    }
}
