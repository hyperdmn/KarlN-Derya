using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject Capsule;
    public float speed;
    public float stopDistance;

    private Transform target;

    void Update()
    {
        // Check for movement input, regardless of dragging state
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check for Derya's movement by using the "Target" tag for waiter
                if (hit.transform.CompareTag("Target"))
                {
                    target = hit.transform;
                }
            }
        }

        // Move the player towards the target
        if (target != null)
        {
            float distance = Vector3.Distance(Capsule.transform.position, target.position);

            if (distance > stopDistance)
            {
                Capsule.transform.position = Vector3.MoveTowards(Capsule.transform.position, target.position, speed * Time.deltaTime);
            }
            else
            {
                target = null;
            }
        }
    }
}
