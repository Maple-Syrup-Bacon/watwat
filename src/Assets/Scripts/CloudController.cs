using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public float speed;
    public float endX;

    // Update is called once per frame
    void Update()
    {
        if (endX < transform.position.x)
        {
            Destroy(gameObject);
        }

        var pos = transform.position;
        pos.x += speed * Time.deltaTime;

        transform.position = pos;
    }
}
