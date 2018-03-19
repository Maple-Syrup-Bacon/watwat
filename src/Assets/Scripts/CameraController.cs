using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 2f;
    public float followSpeed = 2f;
    public float zoomFactor = 1.5f;
    public float minSize = 5f;
    public float maxSize = 15f;

    private Transform[] players;

    void FixedCameraFollowSmooth()
    {
        var midpoint = Vector3.zero;
        var greatestDistance = 1f;
        var numberOfAlivePlayers = 0;

        foreach (var player in GameManager.instance.players)
        {
            if (player.isDead)
            {
                continue;
            }

            midpoint += player.transform.position;
            numberOfAlivePlayers++;

            var myGreatestDistance = 0f;

            foreach (var otherPlayer in GameManager.instance.players)
            {
                if (otherPlayer.isDead || player == otherPlayer)
                {
                    continue;
                }

                var distance = (player.transform.position - otherPlayer.transform.position).magnitude;

                if (myGreatestDistance < distance)
                {
                    myGreatestDistance = distance;
                }
            }

            if (greatestDistance < myGreatestDistance)
            {
                greatestDistance = myGreatestDistance;
            }
        }

        if (numberOfAlivePlayers != 0)
        {
            midpoint /= numberOfAlivePlayers;
        }

        // Distance between objects
        // float distance = (t1.position - t2.position).magnitude;

        // Move camera a certain distance
        Vector3 cameraDestination = midpoint - Camera.main.transform.forward * greatestDistance * zoomFactor;
        cameraDestination.z = -10;

        // Adjust ortho size if we're using one of those
        if (Camera.main.orthographic)
        {
            var sizeTarget = (greatestDistance < minSize ? minSize : (maxSize < greatestDistance ? maxSize : greatestDistance));

            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, sizeTarget, zoomSpeed * Time.unscaledDeltaTime);
        }

        // You specified to use MoveTowards instead of Slerp
        Camera.main.transform.position = Vector3.Slerp(Camera.main.transform.position, cameraDestination, followSpeed * Time.unscaledDeltaTime);

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
