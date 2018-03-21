using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    public float explosiveForce;
    public float damage;
    public int playerID;

    private CircleCollider2D circleCollider;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var body = other.GetComponent<Rigidbody2D>();

        if (body)
        {
            var vec = other.transform.position - transform.position;
            vec = vec.normalized * (vec.magnitude - other.bounds.extents.x);

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

                Debug.Log("Mag: " + vec.magnitude);
                Debug.Log("Rad: " + circleCollider.radius);
                Debug.Log("Ratio: " + ratio);

                if (playerController)
                {
                    playerController.Damage(damage * ratio, vec.normalized * ratio * explosiveForce, playerID);
                }
            }
            else
            {
                body.AddForce(vec.normalized * ratio * explosiveForce, ForceMode2D.Impulse);
            }

        }
    }
}
