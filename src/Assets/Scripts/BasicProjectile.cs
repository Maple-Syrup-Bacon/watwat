using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    public float lifespan = 15.0f;
    public GameObject collisionEffect;

    public Transform Owner { get; set; }

    private float currentTime;

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (lifespan <= currentTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger && other.transform != Owner)
        {
            Debug.Log(other.name);
            Instantiate(collisionEffect, transform.position, collisionEffect.transform.rotation);

            GetComponent<ParticleSystem>().Stop();

            foreach (Transform child in transform)
            {
                var ps = child.GetComponent<ParticleSystem>();

                if (ps)
                {
                    ps.Stop();
                }
            }

            Destroy(GetComponent<Collider2D>());
            Destroy(GetComponent<Rigidbody2D>());
            Destroy(gameObject, 1f);
            Destroy(this);
        }
    }
}
