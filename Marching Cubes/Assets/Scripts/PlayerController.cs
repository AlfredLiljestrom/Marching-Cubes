using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject Manager; 
    public Vector3 planetMidpoint;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        if (Manager == null)
            return; 
        // Calculate the direction to the midpoint
        Vector3 directionToMidpoint = (planetMidpoint - transform.position).normalized;

        // Align the player's rotation to face the midpoint
        Quaternion targetRotation = Quaternion.LookRotation(directionToMidpoint, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        // Move the player
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);
    }

    public void getMidPoint()
    {
        ObjectRenderer ob = Manager.GetComponent<ObjectRenderer>();
        ob.updateObjectList();
        GameObject go = ob.getClosestObject(transform.position);
        planetMidpoint = go.transform.position; 
    }
}
