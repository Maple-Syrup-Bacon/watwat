﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGravity : MonoBehaviour
{
    private Rigidbody2D body;
    private float lastPlanetDistance = float.PositiveInfinity;
    private List<PlanetGravity> planets;
    private float offset;

    private void Start()
    {
        planets = new List<PlanetGravity>();
        body = GetComponent<Rigidbody2D>();
        offset = GetComponent<BoxCollider2D>().bounds.extents.y;
    }

    private void Update()
    {
        var rot = transform.rotation.eulerAngles;
        rot.x = 0.0f;
        rot.y = 0.0f;

        transform.rotation = Quaternion.Euler(rot);
    }

    private void FixedUpdate()
    {
        var closest = Vector3.positiveInfinity;

        foreach (var planet in planets)
        {
            var diff = planet.transform.position - transform.position;

            var orbitDiff = (diff.magnitude - planet.GetPlanetRadius() - offset);
            var ratio = Mathf.Abs(1 - (orbitDiff / planet.GetRadiusDifference()));

            body.AddForce(diff.normalized * planet.GetGravityPull() * ratio * Time.fixedDeltaTime);

            if (diff.magnitude < closest.magnitude)
            {
                closest = diff;
            }
        }

        if (planets.Count != 0)
        {
            transform.up = -closest.normalized;
        }
    }

    public void AddPlanet(PlanetGravity planet)
    {
        planets.Add(planet);
    }

    public void RemovePlanet(PlanetGravity planet)
    {
        planets.Remove(planet);
    }
}
