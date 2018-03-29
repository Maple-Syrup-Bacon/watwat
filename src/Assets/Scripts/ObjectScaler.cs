using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScaler : MonoBehaviour {


	public float minScale;
	public float maxScale;
	// Use this for initialization
	void Start () {
		var scale = Random.Range(minScale, maxScale);
		gameObject.transform.localScale = new Vector3(scale, scale, scale);
	}
}
