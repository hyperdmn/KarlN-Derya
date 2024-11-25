using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform player; // Reference to the player (e.g., Capsule)
    public float speed = 5f; // Speed of movement
    public float stopDistance = 2.5f; // Distance to stop from the target
    public float smoothTime = 0.2f; // Time to smooth the movement

    private Vector3 targetPosition; // Position the player moves to
    private bool isMoving = false; // Is the player currently moving?
    private Vector3 velocity = Vector3.zero; // Used for smoothing

    private Rigidbody rb; // Rigidbody for physics-based movement

    void Start()
    {
        rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Player object must have a Rigidbody component for smooth movement.");
        }
    }

    void Update()
    {
        // Detect mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the clicked object or its parent has the "Target" tag
                Transform clickedObject = hit.transform;
                while (clickedObject != null)
                {
                    if (clickedObject.CompareTag("Target"))
                    {
                        targetPosition = hit.point; // Set the target position to the clicked point
                        isMoving = true; // Start moving
                        break;
                    }
                    clickedObject = clickedObject.parent; // Check parent objects
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            float distance = Vector3.Distance(player.position, targetPosition);

            if (distance > stopDistance)
            {
                // Smoothly move the player towards the target position
                Vector3 smoothedPosition = Vector3.SmoothDamp(player.position, targetPosition, ref velocity, smoothTime);
                rb.MovePosition(smoothedPosition);
            }
            else
            {
                // Stop when within the stopping distance
                isMoving = false;
                rb.velocity = Vector3.zero; // Ensure the player stops completely
            }
        }
    }
}
