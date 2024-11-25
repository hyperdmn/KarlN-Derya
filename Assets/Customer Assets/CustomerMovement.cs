using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        StoreOriginalMaterials();
        originalPosition = transform.position;
    }

    private void StoreOriginalMaterials()
    {
        // Find all table objects
        GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");
        foreach (GameObject table in tables)
        {
            // Get the parent group that contains the table and chairs
            Transform tableGroup = table.transform.parent;
            if (tableGroup != null)
            {
                // Store materials for all renderers in the group
                foreach (Renderer renderer in tableGroup.GetComponentsInChildren<Renderer>())
                {
                    if (!originalMaterials.ContainsKey(renderer.gameObject))
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
        isDragging = true;
        offset = GetMouseWorldPosition() - transform.position;
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = GetMouseWorldPosition() - offset;
        newPosition.y = Mathf.Max(newPosition.y, 0f);
        transform.position = newPosition;
        HighlightTargetTableGroup();
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (TryDropOnTargetTableGroup(out GameObject targetTableGroup))
        {
            if (ValidateTableGroup(targetTableGroup))
            {
                Transform chairTransform = GetAvailableChair(targetTableGroup);
                if (chairTransform != null)
                {
                    Vector3 chairPosition = chairTransform.position;
                    transform.position = new Vector3(chairPosition.x, chairPosition.y + 0.5f, chairPosition.z);
                    transform.SetParent(chairTransform);
                    Debug.Log($"Customer seated at {targetTableGroup.name} on chair {chairTransform.name}");
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

    private void HighlightTargetTableGroup()
    {
        ResetHighlights();
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

            // Highlight everything in the group
            foreach (Renderer renderer in closestTableGroup.GetComponentsInChildren<Renderer>())
            {
                GameObject obj = renderer.gameObject;
                bool isChair = obj.name.ToLower().Contains("chair");
                
                // If it's a chair, check if it's available
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

            Debug.Log($"Highlighting {closestTableGroup.name}, Valid: {isValid}, Available Chairs: {CountAvailableChairs(closestTableGroup)}");
        }
    }

    private bool IsOverTableGroup(GameObject tableGroup)
    {
        // Create a combined bounds from all renderers in the group
        Bounds combinedBounds = new Bounds(tableGroup.transform.position, Vector3.zero);
        foreach (Renderer renderer in tableGroup.GetComponentsInChildren<Renderer>())
        {
            combinedBounds.Encapsulate(renderer.bounds);
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
                    renderer.material = originalMaterials[obj];
                }
            }
        }
        currentHighlightedObjects.Clear();
        currentTableGroup = null;
    }
}