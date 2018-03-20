using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    public float rotationSpeed = 25f;

    public float PlanetRadius { get; set; }

    private float currentRotation;

    private void Start()
    {
        foreach (var collider in GetComponents<CircleCollider2D>())
        {
            if (!collider.isTrigger)
            {
                PlanetRadius = collider.radius * transform.localScale.x;
                break;
            }
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        currentRotation = (currentRotation + rotationSpeed * Time.fixedDeltaTime) % 360f;

        transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed * Time.fixedDeltaTime));
    }
}
