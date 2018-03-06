using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    [SerializeField]
    private GameObject collisionEffect;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Instantiate(collisionEffect, transform.position, collisionEffect.transform.rotation);

            Destroy(GetComponent<Collider>());
            Destroy(GetComponent<Rigidbody>());
            Destroy(gameObject, 1f);
            Destroy(this);
        }
    }
}
