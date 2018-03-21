using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public float speed;
    public float endX;
    public float rotationSpeed;

    private float currentRotation;

    private void Update()
    {
        currentRotation = (currentRotation + rotationSpeed * Time.fixedDeltaTime) % 360f;

        transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed * Time.fixedDeltaTime));

        if (endX < transform.position.x)
        {
            Destroy(gameObject);
        }

        var pos = transform.position;
        pos.x += speed * Time.deltaTime;

        transform.position = pos;
    }
}
