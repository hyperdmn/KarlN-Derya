using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerMovement : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    public bool isDragging;
    private GameObject currentTable;
    private Material defaultMaterial;
    public Material highlightedMaterial;
    public int customerCount = 1; // Number of customers (can adjust this based on game logic)
    private Vector3 originalPosition; // To store the original position of the customer

    private void Start()
    {
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        InitializeTableMaterials();
        originalPosition = transform.position; // Save the starting position
    }

    private void InitializeTableMaterials()
    {
        GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");
        foreach (GameObject table in tables)
        {
            Renderer renderer = table.GetComponent<Renderer>();
            if (renderer != null)
            {
                defaultMaterial = renderer.material;
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
        HighlightTargetTable();
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (TryDropOnTargetTable(out GameObject targetTable))
        {
            if (ValidateTable(targetTable))
            {
                transform.position = targetTable.transform.position;
                Debug.Log("Customer seated at " + targetTable.name);
            }
            else
            {
                Debug.Log("Customer cannot sit at " + targetTable.name + " - not enough seats.");
                transform.position = originalPosition; // Return to original position if they cannot sit
            }
        }

        ResetCurrentTableColor();
    }

    private void HighlightTargetTable()
    {
        ResetCurrentTableColor();
        GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");
        GameObject closestTable = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject table in tables)
        {
            if (IsOverTable(table))
            {
                float distance = Vector3.Distance(transform.position, table.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTable = table;
                }
            }
        }

        if (closestTable != null)
        {
            ColorTable(closestTable, true);
            currentTable = closestTable;
        }
    }

    private bool IsOverTable(GameObject table)
    {
        float distance = Vector3.Distance(transform.position, table.transform.position);
        return distance < 1.5f;
    }

    private bool TryDropOnTargetTable(out GameObject targetTable)
    {
        targetTable = null;
        GameObject[] tables = GameObject.FindGameObjectsWithTag("Table");

        foreach (GameObject table in tables)
        {
            if (IsOverTable(table))
            {
                targetTable = table;
                return true;
            }
        }

        return false;
    }

    private bool ValidateTable(GameObject table)
    {
        // Check if the table fits the customer count
        if (table.name.Contains("Small") && customerCount <= 2)
        {
            return true;
        }
        else if (table.name.Contains("Long") && customerCount <= 4)
        {
            return true;
        }
        return false;
    }

    private void ColorTable(GameObject table, bool isValid)
    {
        Renderer renderer = table.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = isValid ? highlightedMaterial : defaultMaterial;
        }
    }

    private void ResetCurrentTableColor()
    {
        if (currentTable != null)
        {
            ColorTable(currentTable, false);
            currentTable = null;
        }
    }
}
