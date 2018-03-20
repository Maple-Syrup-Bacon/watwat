using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    [SerializeField]
    private float lifespan = 15.0f;

    [SerializeField]
    private GameObject collisionEffect;

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
        if (!other.CompareTag("Player"))
        {
            Instantiate(collisionEffect, transform.position, collisionEffect.transform.rotation);

            if (other.CompareTag("Enemy"))
            {
                Destroy(other.gameObject);
            }

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
