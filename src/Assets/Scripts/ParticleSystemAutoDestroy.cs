using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour
{
    [SerializeField]
    private float lifespan = 15.0f;

    private ParticleSystem ps;
    private float currentTime;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (lifespan <= currentTime || (ps && !ps.IsAlive()))
        {
            Destroy(gameObject);
        }
    }
}
