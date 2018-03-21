using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndExplosion : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Destroy(gameObject);
    }
}
