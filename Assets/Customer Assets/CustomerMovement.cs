using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomerMovement : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    public bool isDragging;
    private GameObject currentTableGroup;
    private List<GameObject> currentHighlightedObjects = new List<GameObject>();
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    public Material highlightedMaterial;
    public Material unavailableMaterial;
    public int customerCount = 1;
    private Vector3 originalPosition;
    private bool isSeated = false;
    private bool isReadyToOrder = false;

    // Order system variables
    private float orderWaitTime = 5f;
    private float orderTimer = 0f;

    // Placeholder for order signal
    public GameObject orderSignalPrefab;
    private GameObject currentOrderSignal;

    private void Start()
    {
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        StoreOriginalMaterials();
        originalPosition = transform.position;
    }

    private void Update()
    {
        // Order timer logic
        if (isSeated && !isReadyToOrder)
        {
            orderTimer += Time.deltaTime;
            
            if (orderTimer >= orderWaitTime)
            {
                ReadyToOrder();
            }
        }
    }

    private void ReadyToOrder()
    {
        isReadyToOrder = true;
        
        // Create order signal
        if (orderSignalPrefab != null)
        {
            // Instantiate order signal above the customer
            Vector3 signalPosition = transform.position + Vector3.up * 2f; // Adjust height as needed
            currentOrderSignal = Instantiate(orderSignalPrefab, signalPosition, Quaternion.identity);
            
            // Optional: Make signal a child of the customer to move with them
            currentOrderSignal.transform.SetParent(transform);
        }

        Debug.Log("Customer is ready to order!");
    }

    private void StoreOriginalMaterials()
    {
        GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");
        foreach (GameObject table in tables)
        {
            Transform tableGroup = table.transform.parent;
            if (tableGroup != null)
            {
                foreach (Renderer renderer in tableGroup.GetComponentsInChildren<Renderer>())
                {
                    if (!originalMaterials.ContainsKey(renderer.gameObject) && renderer.gameObject != this.gameObject)
                    {
                        originalMaterials[renderer.gameObject] = renderer.material;
                    }
                }
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zCoord);
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void OnMouseDown()
    {
        if (!isSeated)
        {
            isDragging = true;
            offset = GetMouseWorldPosition() - transform.position;
        }
    }

    private void OnMouseDrag()
    {
        if (!isSeated)
        {
            Vector3 newPosition = GetMouseWorldPosition() - offset;
            
            // Lock Y-axis to the original Y position
            newPosition.y = transform.position.y;

            transform.position = newPosition;
            HighlightTargetTableGroup();
        }
    }

    private void OnMouseUp()
    {
        if (!isSeated)
        {
            isDragging = false;

            if (TryDropOnTargetTableGroup(out GameObject targetTableGroup))
            {
                if (ValidateTableGroup(targetTableGroup))
                {
                    Transform availableChair = GetAvailableChair(targetTableGroup);
                    if (availableChair != null)
                    {
                        // Reset order timer and state
                        orderTimer = 0f;
                        isReadyToOrder = false;
                        
                        // Precisely position on the available chair
                        PositionOnChair(availableChair);
                        
                        isSeated = true;
                        Debug.Log($"Customer seated at {targetTableGroup.name} on chair {availableChair.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"No available chair found in: {targetTableGroup.name}. Available chairs: {CountAvailableChairs(targetTableGroup)}");
                        transform.position = originalPosition;
                    }
                }
                else
                {
                    Debug.Log($"Customer cannot sit at {targetTableGroup.name} - not enough seats or invalid table size.");
                    transform.position = originalPosition;
                }
            }
            else
            {
                transform.position = originalPosition;
            }

            ResetHighlights();
        }
    }

    private void PositionOnChair(Transform chairTransform)
    {
        // Find the chair's Renderer for bounds calculation
        Renderer chairRenderer = chairTransform.GetComponent<Renderer>();

        if (chairRenderer != null)
        {
            Bounds chairBounds = chairRenderer.bounds;

            // Calculate the top center of the chair
            Vector3 chairTopCenter = new Vector3(
                chairBounds.center.x,
                chairBounds.max.y,
                chairBounds.center.z
            );

            // Position the customer precisely on the top center of the chair
            transform.position = chairTopCenter + (Vector3.up * (transform.localScale.y / 2f));
        }
        else
        {
            // Fallback to positioning based on the chair's transform
            transform.position = chairTransform.position + (Vector3.up * (transform.localScale.y / 2f));
        }

        // Set the customer's parent to the chair transform
        transform.SetParent(chairTransform);

        // Ensure the customer is oriented correctly
        transform.localRotation = Quaternion.identity;
    }

    private Transform FindSittingPositionMarker(Transform chairTransform)
    {
        // Look for a child object or component named "SittingPositionMarker"
        Transform marker = chairTransform.Find("SittingPositionMarker");
        
        if (marker == null)
        {
            // If no marker found directly on the chair, search in children
            marker = chairTransform.GetComponentsInChildren<Transform>()
                .FirstOrDefault(t => t.name == "SittingPositionMarker");
        }

        return marker;
    }

    private void PreciselyPositionOnChair(Transform chairTransform)
    {
        // Fallback positioning method
        Renderer chairRenderer = chairTransform.GetComponent<Renderer>();
        
        if (chairRenderer != null)
        {
            Bounds chairBounds = chairRenderer.bounds;
            
            Vector3 chairTopCenter = new Vector3(
                chairBounds.center.x, 
                chairBounds.max.y, 
                chairBounds.center.z
            );

            // Position the customer precisely at the top center of the chair
            transform.position = chairTopCenter + (Vector3.up * (transform.localScale.y / 2f));
            transform.SetParent(chairTransform);
        }
        else
        {
            // Last resort positioning
            transform.position = chairTransform.position + (Vector3.up * (transform.localScale.y / 2f));
            transform.SetParent(chairTransform);
        }
    }

    private void HighlightTargetTableGroup()
    {
        ResetHighlights();

        if (isDragging)
        {
            GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");
            GameObject closestTableGroup = null;
            float closestDistance = float.MaxValue;

            foreach (GameObject table in tables)
            {
                Transform tableGroup = table.transform.parent;
                if (tableGroup != null && IsOverTableGroup(tableGroup.gameObject))
                {
                    float distance = Vector3.Distance(transform.position, tableGroup.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTableGroup = tableGroup.gameObject;
                    }
                }
            }

            if (closestTableGroup != null)
            {
                currentTableGroup = closestTableGroup;
                bool isValid = ValidateTableGroup(closestTableGroup);

                foreach (Renderer renderer in closestTableGroup.GetComponentsInChildren<Renderer>())
                {
                    GameObject obj = renderer.gameObject;

                    // Highlight only table and chair objects
                    if (obj.CompareTag("Table") || obj.name.ToLower().Contains("chair"))
                    {
                        bool isChair = obj.name.ToLower().Contains("chair");

                        if (isChair)
                        {
                            bool isChairAvailable = !HasCustomer(obj.transform);
                            ColorObject(obj, isValid && isChairAvailable);
                        }
                        else
                        {
                            ColorObject(obj, isValid);
                        }
                        currentHighlightedObjects.Add(obj);
                    }
                }

                Debug.Log($"Highlighting {closestTableGroup.name}, Valid: {isValid}, Available Chairs: {CountAvailableChairs(closestTableGroup)}");
            }
        }
    }

    private bool IsOverTableGroup(GameObject tableGroup)
    {
        // Create a combined bounds from all renderers in the group
        Bounds combinedBounds = new Bounds(tableGroup.transform.position, Vector3.zero);
        foreach (Renderer renderer in tableGroup.GetComponentsInChildren<Renderer>())
        {
            // Exclude the customer from bounds calculation
            if (renderer.gameObject != this.gameObject)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }
        
        // Expand bounds for easier detection
        combinedBounds.Expand(2.5f);
        return combinedBounds.Contains(transform.position);
    }

    private bool TryDropOnTargetTableGroup(out GameObject targetTableGroup)
    {
        targetTableGroup = null;
        GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");
        float closestDistance = float.MaxValue;

        foreach (GameObject table in tables)
        {
            Transform group = table.transform.parent;
            if (group != null && IsOverTableGroup(group.gameObject))
            {
                float distance = Vector3.Distance(transform.position, group.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetTableGroup = group.gameObject;
                }
            }
        }

        return targetTableGroup != null;
    }

    private bool ValidateTableGroup(GameObject tableGroup)
    {
        if (tableGroup == null) return false;

        string groupName = tableGroup.name.ToLower();
        int availableChairs = CountAvailableChairs(tableGroup);
        Debug.Log($"Validating {groupName} - Available chairs: {availableChairs}, Required: {customerCount}");

        // Check if the table is appropriate for the customer count and has enough available chairs
        if (groupName.Contains("small") && customerCount <= 2 && availableChairs >= customerCount)
        {
            return true;
        }
        else if (groupName.Contains("long") && customerCount <= 4 && availableChairs >= customerCount)
        {
            return true;
        }
        return false;
    }

    private Transform GetAvailableChair(GameObject tableGroup)
    {
        if (tableGroup == null) return null;

        // Find all chair objects in the group
        foreach (Transform child in tableGroup.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower().Contains("chair") && !HasCustomer(child))
            {
                return child;
            }
        }
        return null;
    }

    private bool HasCustomer(Transform chair)
    {
        return chair.childCount > 0 && chair.GetComponentInChildren<CustomerMovement>() != null;
    }

    private int CountAvailableChairs(GameObject tableGroup)
    {
        int count = 0;
        foreach (Transform child in tableGroup.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower().Contains("chair") && !HasCustomer(child))
            {
                count++;
            }
        }
        Debug.Log($"Found {count} available chairs in {tableGroup.name}");
        return count;
    }

    private void ColorObject(GameObject obj, bool isValid)
    {
        if (obj == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = isValid ? highlightedMaterial : unavailableMaterial;
        }
    }

    private void ResetHighlights()
    {
        foreach (GameObject obj in currentHighlightedObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && originalMaterials.ContainsKey(obj))
                {
                    // Reset only table and chair materials, not customers
                    if (obj.CompareTag("Table") || obj.name.ToLower().Contains("chair"))
                    {
                        renderer.material = originalMaterials[obj];
                    }
                }
            }
        }
        currentHighlightedObjects.Clear();
        currentTableGroup = null;
    }

    public void ResetCustomer()
    {
        // Destroy order signal if it exists
        if (currentOrderSignal != null)
        {
            Destroy(currentOrderSignal);
        }

        isSeated = false;
        isReadyToOrder = false;
        orderTimer = 0f;

        // Store the original parent before detaching
        Transform originalParent = transform.parent;

        // Detach from the chair
        transform.SetParent(null);

        // Reset chair material
        if (originalParent != null && originalMaterials.ContainsKey(originalParent.gameObject))
        {
            Renderer parentRenderer = originalParent.GetComponent<Renderer>();
            if (parentRenderer != null)
            {
                parentRenderer.material = originalMaterials[originalParent.gameObject];
            }
        }

        // Move the customer back to the original position
        transform.position = originalPosition;
    }
}
