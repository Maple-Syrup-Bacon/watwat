using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{

    public float minSpeed;
    public float maxSpeed;
    private Vector3 direction;
    private float movementSpeed;

    private Rigidbody2D body;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        movementSpeed = Random.Range(minSpeed, maxSpeed);
        direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        body.AddForce(direction * movementSpeed, ForceMode2D.Impulse);
    }

    void Update()
    {
        // transform.position += direction * movementSpeed * Time.fixedDeltaTime;
    }
}
