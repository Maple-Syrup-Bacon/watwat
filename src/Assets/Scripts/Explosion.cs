using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EazyTools.SoundManager;

public class Explosion : MonoBehaviour
{

    public float explosiveForce;
    public float damage;
    public int playerID;

    [Header("Camera Shake")]
    public float shakeMagnitude = 50;
    public float shakeRoughness = 10;
    public float shakeFadeIn = 0.25f;
    public float shakeFadeOut = 1;
    [Header("Audio")]
    public AudioClip explosion;
    public float explosionVolume = 1.0f;

    private CircleCollider2D circleCollider;
    private List<Rigidbody2D> hitBodies;
    private Vector2 pos;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        EZCameraShake.CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, shakeFadeIn, shakeFadeOut);
        hitBodies = new List<Rigidbody2D>();

        pos = new Vector2(transform.position.x, transform.position.y);

        SoundManager.PlaySound(explosion, explosionVolume);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var body = other.GetComponent<Rigidbody2D>();

        if (body && !hitBodies.Contains(body))
        {
            hitBodies.Add(body);

            var otherVec = new Vector2(other.transform.position.x, other.transform.position.y);

            var vec = otherVec - pos;

            var ratio = 0f;

            if (circleCollider.radius < vec.magnitude)
            {
                ratio = 0.1f;
            }
            else
            {
                ratio = 1 - (vec.magnitude / circleCollider.radius);
            }

            if (other.CompareTag("Player"))
            {
                var playerController = other.GetComponent<PlayerController>();

                if (playerController.playerID == playerID)
                {
                    return;
                }

                if (playerController)
                {
                    playerController.Damage(damage * ratio, vec, playerID);
                }
            }

            body.AddForce(vec.normalized * ratio * explosiveForce, ForceMode2D.Impulse);
        }
    }
}
