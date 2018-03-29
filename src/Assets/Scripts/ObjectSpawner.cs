using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour {

	public GameObject[] prefabs;
	public float spawnRate = 0.8f;	
	public float xStartMin = -60f;
	public float xStartMax = -60f;
	public float yStartMin = 60f;
	public float yStartMax = 60f;

	void Start () {
		StartCoroutine(SpawnObjects());
	}
	
	public IEnumerator SpawnObjects(){
		while(true){
			yield return new WaitForSeconds(spawnRate);
			if(prefabs.Length > 0){
				var pos = new Vector3(Random.Range(xStartMin, xStartMax), Random.Range(yStartMin, yStartMax), 0f);			
				var index = Random.Range(0, prefabs.Length);
				var obj = Instantiate(prefabs[index], pos, prefabs[index].transform.rotation);
			} else {
				break;
			}
		}
	}
}
