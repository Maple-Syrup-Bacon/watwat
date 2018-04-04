using System.Collections;
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
    public float groundCheckLength = 0.1f;
    public float movementSpeed = 10f;
    public float jumpForce = 10f;
    public float degroundedTime = 0.25f;
    public float maxSpeed = 200f;
    public float lightProjectileSpeed = 50f;
    public float lightProjectileCooldown = 1.5f;
    public float meleeDamage = 10f;
    public float meleeCooldown = 1f;
    public float invincibilityFrameTime = 0.1f;
    public GameObject fireballProjectile;

    [Header("Dash")]
    public float dashForce = 100f;
    public float dashDuration = 1f;
    public float dashDrag = 1f;

    [Header("Powerup Effects")]
    public float superStrengthEffect = 4.0f;
    public float superSpeedEffect = 2.0f;

    [Header("Particles")]
    public GameObject groundedParticle;
    public GameObject meleeHitParticle;
    public GameObject meleeParticle;
    public GameObject deathParticle;
    public GameObject[] spawnParticles;
    public GameObject[] dashParticles;

    [Header("Camera Shake")]
    public float deathMagnitude = 100;
    public float deathRoughness = 10;
    public float deathFadeIn = 0.25f;
    public float deathFadeOut = 1;


    [Header("Audio")]
    public AudioClip death;
    public float deathVolume = 1.0f;
    public AudioClip spawnPortal;
    public float spawnPortalVolume = 1.0f;
    public AudioClip spawn;
    public float spawnVolume = 1.0f;
    public AudioClip fireball;
    public float fireballVolume = 1.0f;
    public AudioClip meleeHit;
    public float meleeHitVolume = 1.0f;
    public AudioClip meleeMiss;
    public float meleeMissVolume = 1.0f;
    public AudioClip dash;
    public float dashVolume = 1.0f;
    public AudioClip superSpeedDash;
    public float superSpeedDashVolume = 1.0f;

    // Properties
    public bool IsGrounded { get; set; }
    public bool IsVisible { get; set; }
    public PlanetGravity CurrentPlanet { get; set; }
    public Sprite IdleSprite { get; set; }
    public RuntimeAnimatorController AnimatorController { get; set; }
    public bool dashDisabled { get; set; } = false;
    public _2dxFX_LightningBolt SuperSpeedSpriteEffect { get; set; }
    public _2dxFX_Fire FireballSpriteEffect { get; set; }
    [SerializeField]
    private Rewired.Player player;
    [SerializeField]
    private Rigidbody2D body;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    private ObjectGravity objectGravity;
    [SerializeField]
    private Transform aimerTransform;
    [SerializeField]
    private SpriteRenderer aimerSpriteRenderer;
    [SerializeField]
    private SpriteRenderer weaponIdleSpriteRenderer;
    [SerializeField]
    private SpriteRenderer weaponAttackSpriteRenderer;
    private Transform weaponTransform;
    private Transform weaponJoint;
    [SerializeField]
    private Sprite[] idleWeaponSprites;
    [SerializeField]
    private Sprite[] attackWeaponSprites;
    private float weaponXRight;
    private float weaponJointXRight;
    [SerializeField]
    private TrailRenderer trailRenderer;
    private ParticleSystem.MainModule meleeParticlePrefabMain;
    [SerializeField]
    private BoxCollider2D boxCollider;

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
    [SerializeField]
    private PlayerAttack playerAttack;
    private float attackTriggerXRight;
    private float attackTriggerXLeft;
    private bool isFacingRight = true;
    private bool degrounded = false;
    private float degroundedTimer;
    private bool inInvincibilityFrame = false;
    private float invincibilityFrameTimer;
    private int lastDamagedByPlayerID = -1;
    private float lastDamageTime;
    private int planetLayer;

    // Powerups
    public bool HasFireball { get; set; } = false;
    public bool HasInvincibility { get; set; } = false;
    public bool HasSuperStrength { get; set; } = false;
    public bool HasSuperSpeed { get; set; } = false;

    private void Awake()
    {
        // body = GetComponent<Rigidbody2D>();
        // animator = GetComponentInChildren<Animator>();
        // spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    private void Start()
    {
        player = ReInput.players.GetPlayer(playerID);
        // aimerTransform = transform.GetChild(0);
        // playerAttack = aimerTransform.GetComponent<PlayerAttack>();
        // aimerSpriteRenderer = aimerTransform.GetComponent<SpriteRenderer>();
        meleeParticlePrefabMain = meleeParticle.GetComponent<ParticleSystem>().main;
        // trailRenderer = GetComponent<TrailRenderer>();
        // boxCollider = GetComponent<BoxCollider2D>();

        weaponTransform = weaponIdleSpriteRenderer.transform;
        weaponJoint = weaponTransform.parent.transform;
        weaponXRight = weaponTransform.localPosition.x;
        weaponJointXRight = weaponJoint.localPosition.x;
        weaponIdleSpriteRenderer.sprite = idleWeaponSprites[playerID];
        weaponIdleSpriteRenderer.enabled = true;
        weaponAttackSpriteRenderer.sprite = attackWeaponSprites[playerID];
        weaponAttackSpriteRenderer.enabled = false;

        originalFixedDelta = Time.fixedDeltaTime;
        leftAnalogStick = new Vector2();
        sqrMaxSpeed = Mathf.Pow(maxSpeed, 2);
        attackTriggerXRight = playerAttack.transform.localPosition.x;
        attackTriggerXLeft = -attackTriggerXRight;
        aimerVector = Vector2.right;
        IsVisible = true;
        animator.runtimeAnimatorController = AnimatorController;
        spriteRenderer.sprite = IdleSprite;

        SuperSpeedSpriteEffect = GetComponent<_2dxFX_LightningBolt>();
        SuperSpeedSpriteEffect.enabled = false;

        FireballSpriteEffect = GetComponent<_2dxFX_Fire>();
        FireballSpriteEffect.enabled = false;
        planetLayer = 1 << LayerMask.NameToLayer("Planet");
    }

    // Update is called once per frame
    private void Update()
    {
        if (player.GetButtonDown("Pause") && !GameManager.instance.GameOver)
        {
            GameManager.instance.TogglePause(playerID);
        }

        if (isDead || GameManager.instance.Paused)
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
        if (GameManager.instance.GameOver)
        {
            body.velocity = Vector2.zero;
        }

        if (isDead || GameManager.instance.Paused)
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

                weaponIdleSpriteRenderer.flipX = false;
                weaponJoint.localPosition = new Vector3(weaponJointXRight, weaponJoint.localPosition.y, weaponJoint.localPosition.z);
                weaponTransform.localPosition = new Vector3(weaponXRight, weaponTransform.localPosition.y, weaponTransform.localPosition.z);
                foreach (Transform go in weaponTransform)
                {
                    go.localPosition = new Vector3(weaponXRight - 0.5f, go.localPosition.y, go.localPosition.z);
                }
            }
            else
            {
                spriteRenderer.flipX = true;
                attackTriggerPos.x = attackTriggerXLeft;

                weaponIdleSpriteRenderer.flipX = true;
                weaponJoint.localPosition = new Vector3(-weaponJointXRight, weaponJoint.localPosition.y, weaponJoint.localPosition.z);
                weaponTransform.localPosition = new Vector3(-weaponXRight, weaponTransform.localPosition.y, weaponTransform.localPosition.z);
                foreach (Transform go in weaponTransform)
                {
                    go.localPosition = new Vector3(-weaponXRight + 0.5f, go.localPosition.y, go.localPosition.z);
                }
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

                Instantiate(dashParticles[playerID], transform.position, dashParticles[playerID].transform.rotation);

                if (HasSuperSpeed)
                {
                    SoundManager.PlaySound(superSpeedDash, superSpeedDashVolume);
                }
                else
                {
                    SoundManager.PlaySound(dash, dashVolume);
                }
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

        if (lastDamagedByPlayerID != -1 && Time.time - GameManager.instance.lastTouchDuration <= lastDamageTime)
        {
            GameManager.instance.IncreaseScore(lastDamagedByPlayerID);
            lastDamagedByPlayerID = -1;
        }
        else
        {
            GameManager.instance.DecrementScore(playerID);
        }

        EZCameraShake.CameraShaker.Instance.ShakeOnce(deathMagnitude, deathRoughness, deathFadeIn, deathFadeOut);
        SoundManager.PlaySound(death, deathVolume);
        PowerupManager.instance.DisablePowerups(this);

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        spriteRenderer.enabled = false;
        aimerSpriteRenderer.enabled = false;
        weaponAttackSpriteRenderer.enabled = false;
        weaponIdleSpriteRenderer.enabled = false;
        trailRenderer.enabled = false;

        yield return new WaitForSeconds(GameManager.instance.timeVisibleAfterDeath);

        IsVisible = false;

        yield return new WaitForSeconds(GameManager.instance.respawnTime - GameManager.instance.timeVisibleAfterDeath);

        if (!GameManager.instance.GameOver)
        {
            // Spawn Particles and set spawn position, zero out velocity and set visible for camera focus
            var index = Random.Range(0, GameManager.instance.Planets.Length);
            var planet = GameManager.instance.Planets[index];
            var planetRadius = planet.GetComponent<CircleCollider2D>().radius;
            var planetPos = planet.transform.position;
            var planetPosX = Random.Range(0, 2) == 0 ? planetPos.x - planetRadius : planetPos.x + planetRadius;
            var planetPosY = Random.Range(0, 2) == 0 ? planetPos.y - planetRadius : planetPos.y + planetRadius;
            var startPos = new Vector3(planetPosX, planetPosY, 0);
            transform.position = startPos;
            body.velocity = new Vector2(0f, 0f);
            IsVisible = true;
            var instance = Instantiate(spawnParticles[playerID], transform.position, spawnParticles[playerID].transform.rotation);
            var portalAudio = SoundManager.GetAudio(SoundManager.PlaySound(spawnPortal, spawnPortalVolume));

            yield return new WaitForSeconds(GameManager.instance.respawnParticleDelay);

            portalAudio.Stop();
            SoundManager.PlaySound(spawn, spawnVolume);
            transform.position = startPos;
            isDead = false;
            dashDisabled = false;
            spriteRenderer.enabled = true;
            aimerSpriteRenderer.enabled = true;
            weaponIdleSpriteRenderer.enabled = true;
            trailRenderer.enabled = true;

            damageTotal = 0;

            GameManager.instance.SetAvatarPercentage(playerID, (int)damageTotal);
        }
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
            StartCoroutine(WeaponAnimation());

            if (HasFireball)
            {
                SoundManager.PlaySound(fireball, fireballVolume);
                var instance = Instantiate(fireballProjectile, aimerTransform.position, fireballProjectile.transform.rotation);
                instance.GetComponent<BasicProjectile>().Owner = transform;
                instance.GetComponent<Rigidbody2D>().AddForce(aimerVector * lightProjectileSpeed, ForceMode2D.Impulse);
                PowerupManager.instance.DisableFireball(this);
            }
            else
            {
                var rot = -(aimerTransform.rotation.eulerAngles.z + 90);
                meleeParticlePrefabMain.startRotation = new ParticleSystem.MinMaxCurve(rot * Mathf.Deg2Rad);

                var meleeParticeInstance = Instantiate(meleeParticle, aimerTransform.position, Quaternion.identity);

                var damage = (HasSuperStrength ? meleeDamage * superStrengthEffect : meleeDamage);

                if (playerAttack.players.Count == 0)
                {
                    SoundManager.PlaySound(meleeMiss, meleeMissVolume);
                }
                else
                {
                    foreach (var player in playerAttack.players)
                    {
                        player.Damage(damage, player.transform.position - transform.position, playerID, body.velocity);
                        SoundManager.PlaySound(meleeHit, meleeHitVolume);
                    }
                }
            }
        }
    }

    private IEnumerator WeaponAnimation()
    {
        weaponIdleSpriteRenderer.enabled = false;
        weaponAttackSpriteRenderer.enabled = true;
        var aimerAngle = Mathf.Atan2(aimerVector.y, aimerVector.x) * Mathf.Rad2Deg;
        weaponJoint.rotation = Quaternion.Euler(0, 0, aimerAngle);

        yield return new WaitForSeconds(meleeCooldown);

        weaponIdleSpriteRenderer.enabled = true;
        weaponAttackSpriteRenderer.enabled = false;
        weaponJoint.localRotation = Quaternion.Euler(0, 0, 0);
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
            var end = start;
            end -= GetYExtend() * transform.up;
            end -= transform.up.normalized * groundCheckLength;

            Debug.DrawLine(start, end, Color.red);
            var ray = Physics2D.Linecast(start, end, planetLayer);

            if (ray.collider != null && ray.collider.CompareTag("Planet"))
            {
                if (!IsGrounded)
                {
                    var rot = Quaternion.Euler(transform.rotation.eulerAngles.z - 90, groundedParticle.transform.rotation.eulerAngles.y, groundedParticle.transform.rotation.eulerAngles.z);

                    Instantiate(groundedParticle, ray.point, rot);
                }

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

        aimerTransform.position = transform.position + (aimerVector * transform.lossyScale.y);
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
        lastDamageTime = Time.time;

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

        GameManager.instance.SetAvatarPercentage(playerID, (int)damageTotal);
    }

    public void SetGrounded(bool grounded)
    {
        this.IsGrounded = grounded;
    }

    public void SetScale(Vector3 scale)
    {
        var parent = transform.parent;
        transform.parent = null;
        transform.localScale = scale;
        transform.parent = parent;
    }

    public float GetYExtend()
    {
        var yExtend = (boxCollider.size.y / 2) * transform.lossyScale.y;
        yExtend -= boxCollider.offset.y * transform.lossyScale.y;
        return yExtend;
    }
}
