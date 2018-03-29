using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour {

	public float minSpeed;
	public float maxSpeed;
	public float minRotation;
	public float maxRotation;
	private Vector3 direction;
	private float rotationSpeed;
	private float movementSpeed;

	void Start () {
		movementSpeed = Random.Range(minSpeed, maxSpeed);
		rotationSpeed = Random.Range(minRotation, maxRotation);
		direction = new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), 0);
	}
	
	void Update () {
		transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed * Time.fixedDeltaTime));
		transform.position += direction * movementSpeed * Time.fixedDeltaTime;
	}
}
