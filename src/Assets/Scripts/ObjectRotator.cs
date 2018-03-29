using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour {

	public float minRotation;
	public float maxRotation;
	private float rotationSpeed;
	void Start () {
		rotationSpeed = Random.Range(minRotation, maxRotation);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(new Vector3(0.0f, 0.0f, rotationSpeed * Time.fixedDeltaTime));
	}
}
