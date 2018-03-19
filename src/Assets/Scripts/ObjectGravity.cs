using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGravity : MonoBehaviour
{
    public bool useRatio = false;

    private Rigidbody2D body;
    private PlayerController playerController;
    private float lastPlanetDistance = float.PositiveInfinity;
    private List<PlanetGravity> planets;
    private float offset;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        planets = new List<PlanetGravity>();
        body = GetComponent<Rigidbody2D>();
        offset = GetComponent<BoxCollider2D>().bounds.extents.y;
    }

    private void Update()
    {
        // var rot = transform.rotation.eulerAngles;
        // rot.x = 0.0f;
        // // rot.y = 0.0f;

        // transform.rotation = Quaternion.Euler(rot);
    }

    private void FixedUpdate()
    {
        if (playerController && playerController.isDead)
        {
            return;
        }

        if (playerController && playerController.IsGrounded)
        {
            GroundedPlayer();
        }
        else
        {
            ObjectInSpace();
        }

    }

    private void GroundedPlayer()
    {
        var diff = playerController.CurrentPlanet.transform.position - transform.position;

        transform.up = -diff.normalized;

        body.AddForce(diff.normalized * playerController.CurrentPlanet.GetGravityPull() * Time.fixedDeltaTime);
    }

    private void ObjectInSpace()
    {
        var closest = Vector3.positiveInfinity;

        foreach (var planet in planets)
        {
            var diff = planet.transform.position - transform.position;

            var orbitDiff = (diff.magnitude - planet.GetPlanetRadius() - offset);
            var ratio = Mathf.Abs(1 - (orbitDiff / planet.GetRadiusDifference()));

            body.AddForce(diff.normalized * planet.GetGravityPull() * (useRatio ? ratio : 1) * Time.fixedDeltaTime);

            if (diff.magnitude < closest.magnitude)
            {
                closest = diff;
            }
        }

        if (planets.Count != 0)
        {
            transform.up = -closest.normalized;
        }
        else
        {
            // transform.up = body.velocity;
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
