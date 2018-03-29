using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour {

	[Tooltip("If no collider set, generally the case with prefabs then destroy trigger is a Collider2D with Tag 'ObjectBorder'")]
	public Collider2D destroyCollider;

	private void OnTriggerExit2D(Collider2D other)
    {
        if(other == destroyCollider)
        {
            Destroy(gameObject);
        } else if(other.CompareTag("ObjectBorder")){
			Destroy(gameObject);
		}
    }
}
