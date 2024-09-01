using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 20f;    // Speed at which the player moves forward and backward
    public float rotateSpeed = 100f; // Speed at which the player rotates
    public float jumpForce = 5f; 
    public Rigidbody rb;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // Handle movement input
        float moveInputVertical = Input.GetAxis("Vertical");   // W and S keys or Up and Down arrow keys
        float moveInputHorizontal = Input.GetAxis("Horizontal"); // A and D keys or Left and Right arrow keys

        // Calculate movement direction
        Vector3 moveDirection = transform.forward * moveInputVertical + transform.right * moveInputHorizontal;

        // Normalize the direction to prevent faster diagonal movement
        moveDirection.Normalize();

        // Apply movement
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

    }
}
