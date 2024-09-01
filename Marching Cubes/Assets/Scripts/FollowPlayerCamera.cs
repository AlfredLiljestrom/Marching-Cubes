using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public float mouseSensitivity = 100f;  // Sensitivity of the mouse movement
    public Transform playerBody;           // Reference to the player's body

    private float xRotation = 0f;          // Current vertical rotation of the camera
    private float yRotation = 0f;
    public Vector3 offset; 

    void Start()
    {
        // Ensure the camera is aligned with the player at the start
        transform.position = playerBody.position + new Vector3(0, 1.7f, 0); // Adjust the Y offset if needed (1.7f simulates the height of a character)
        transform.rotation = playerBody.rotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        { 
            Cursor.lockState ^= CursorLockMode.Locked;
        } 

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Step 1: Get mouse movement input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Step 2: Rotate the player body around the Y axis (horizontal rotation)
            playerBody.Rotate(Vector3.up * mouseX);

            //// Step 3: Calculate and clamp the camera's vertical rotation (pitch)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Prevent the camera from rotating too far up or down

            yRotation += mouseX;
        }

        


        //// Step 4: Apply the rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        //// Step 5: Ensure the camera's position follows the player
        transform.position = playerBody.position + offset; // Adjust the Y offset if needed
        

    }
}
