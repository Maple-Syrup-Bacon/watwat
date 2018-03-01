using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityScale3D : MonoBehaviour
{

    [SerializeField]
    private float gravityScale = 1.0f;

    private Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    private void FixedUpdate()
    {
        body.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }
}
