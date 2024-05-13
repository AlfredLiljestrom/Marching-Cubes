using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    public GameObject playerTransform;
    public Vector3 offset; // Offset from the player's position

    void LateUpdate()
    {
        // Check if the player transform is assigned
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform is not assigned!");
            return;
        }

        // Update the camera's position based on the player's position and offset
        transform.position = playerTransform.transform.position + offset;

        // Make the camera look at the player
        transform.LookAt(playerTransform.transform);
    }
}
