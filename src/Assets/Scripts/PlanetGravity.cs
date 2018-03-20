using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    [SerializeField]
    private float gravityPull = 100f;

    [SerializeField]
    private float rotationSpeed = 25f;

    private float rotation;
    private CircleCollider2D circleCollider;
    private CircleCollider2D circleTrigger;
    private float radiusDiff;

    private void Start()
    {
        var colliders = GetComponents<CircleCollider2D>();

        foreach (var coll in colliders)
        {
            if (coll.isTrigger)
            {
                circleTrigger = coll;
            }
            else
            {
                circleCollider = coll;
            }
        }

        radiusDiff = circleTrigger.radius - circleCollider.radius;
    }

    private void FixedUpdate()
    {
        rotation = (rotation + rotationSpeed * Time.fixedDeltaTime) % 360f;

        transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed * Time.fixedDeltaTime));
    }

    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("Player"))
    //     {
    //         other.transform.SetParent(transform);
    //     }
    // }

    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     if (other.gameObject.CompareTag("Player"))
    //     {
    //         other.transform.parent = null;
    //     }
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // other.GetComponent<ObjectGravity>().AddPlanet(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // other.GetComponent<ObjectGravity>().RemovePlanet(this);
        }
    }

    public float GetGravityPull()
    {
        return gravityPull;
    }

    public float GetRadiusDifference()
    {
        return radiusDiff;
    }

    public float GetPlanetRadius()
    {
        return circleCollider.radius;
    }
}
