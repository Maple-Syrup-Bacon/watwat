using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float zoomFactor = 1.5f;

    [SerializeField]
    private float minSize = 5f;

    [SerializeField]
    private float maxSize = 15f;

    private Transform[] players;

    void FixedCameraFollowSmooth()
    {
        var midpoint = Vector3.zero;
        var greatestDistance = 0f;

        foreach (var player in GameManager.instance.players)
        {
            midpoint += player.transform.position;

            var myGreatestDistance = 0f;

            foreach (var otherPlayer in GameManager.instance.players)
            {
                if (player != otherPlayer)
                {
                    var distance = (player.transform.position - otherPlayer.transform.position).magnitude;

                    if (myGreatestDistance < distance)
                    {
                        myGreatestDistance = distance;
                    }
                }
            }

            if (greatestDistance < myGreatestDistance)
            {
                greatestDistance = myGreatestDistance;
            }
        }

        midpoint /= GameManager.instance.players.Length;

        // Distance between objects
        // float distance = (t1.position - t2.position).magnitude;

        // Move camera a certain distance
        Vector3 cameraDestination = midpoint - Camera.main.transform.forward * greatestDistance * zoomFactor;

        // Adjust ortho size if we're using one of those
        if (Camera.main.orthographic)
        {
            // The camera's forward vector is irrelevant, only this size will matter
            Camera.main.orthographicSize = (greatestDistance < minSize ? minSize : (maxSize < greatestDistance ? maxSize : greatestDistance));
        }

        // You specified to use MoveTowards instead of Slerp
        Camera.main.transform.position = Vector3.Slerp(Camera.main.transform.position, cameraDestination, Time.unscaledDeltaTime);

        // Snap when close enough to prevent annoying slerp behavior
        if ((cameraDestination - Camera.main.transform.position).magnitude <= 0.05f)
        {
            Camera.main.transform.position = cameraDestination;
        }
    }

    void Update()
    {
        FixedCameraFollowSmooth();
    }
}
