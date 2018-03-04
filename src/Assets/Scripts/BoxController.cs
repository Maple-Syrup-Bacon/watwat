using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;
    [SerializeField]
    private float maxDist = 5f;
    [SerializeField]
    private float minDistance = 1f;

    private Rigidbody body;
    private Vector3 right;
    private Vector3 left;
    private bool goingRight = true;

    // Use this for initialization
    void Start()
    {
        body = GetComponent<Rigidbody>();

        right = transform.position;
        right.x += maxDist;

        left = transform.position;
        left.x -= maxDist;
    }

    void FixedUpdate()
    {
        if (goingRight)
        {
            body.MovePosition(Vector3.Lerp(transform.position, right, speed * Time.fixedDeltaTime));

            if (Vector3.Distance(transform.position, right) < minDistance)
            {
                goingRight = false;
            }
        }
        else
        {
            body.MovePosition(Vector3.Lerp(transform.position, left, speed * Time.fixedDeltaTime));

            if (Vector3.Distance(transform.position, left) < minDistance)
            {
                goingRight = true;
            }
        }
    }
}
