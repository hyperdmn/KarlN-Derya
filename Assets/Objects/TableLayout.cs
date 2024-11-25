using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TableLayout : MonoBehaviour
{
    [System.Serializable]
    public class TableChairSet
    {
        public GameObject tableObject;
        public GameObject[] chairObjects;
    }

    [Header("Table and Chair Sets")]
    public TableChairSet longTableSet;
    public TableChairSet smallTableSet;

    [Header("Spacing Settings")]
    public float chairDistanceFromTable = 0.5f; // Distance between chair and table edge
    public float longTableChairSpacing = 0.8f;  // Spacing between chairs for long table
    public float tableSpacing = 2.0f;           // Spacing between different table sets

    public void AlignTableAndChairs(TableChairSet set, Vector3 position)
    {
        if (set.tableObject == null || set.chairObjects == null) return;

        // Position the table
        set.tableObject.transform.position = position;
        
        // Get table bounds
        Bounds tableBounds = GetObjectBounds(set.tableObject);
        float tableWidth = tableBounds.size.x;
        float tableDepth = tableBounds.size.z;

        // Position chairs based on table type
        if (set.chairObjects.Length == 4) // Long table
        {
            // Position chairs along the length of the table
            for (int i = 0; i < set.chairObjects.Length; i++)
            {
                if (set.chairObjects[i] == null) continue;

                float xOffset = (i < 2) ? -tableWidth/2 : tableWidth/2;
                float zOffset = (i % 2 == 0) ? -longTableChairSpacing : longTableChairSpacing;

                Vector3 chairPosition = position + new Vector3(
                    xOffset * (1 + chairDistanceFromTable),
                    0,
                    zOffset
                );

                set.chairObjects[i].transform.position = chairPosition;

                // Rotate chairs to face the table
                float rotationY = (xOffset < 0) ? 90 : -90;
                set.chairObjects[i].transform.rotation = Quaternion.Euler(0, rotationY, 0);
            }
        }
        else if (set.chairObjects.Length == 2) // Small table
        {
            // Position chairs on opposite sides of the table
            for (int i = 0; i < set.chairObjects.Length; i++)
            {
                if (set.chairObjects[i] == null) continue;

                float xOffset = (i == 0) ? -tableWidth/2 : tableWidth/2;

                Vector3 chairPosition = position + new Vector3(
                    xOffset * (1 + chairDistanceFromTable),
                    0,
                    0
                );

                set.chairObjects[i].transform.position = chairPosition;

                // Rotate chairs to face the table
                float rotationY = (xOffset < 0) ? 90 : -90;
                set.chairObjects[i].transform.rotation = Quaternion.Euler(0, rotationY, 0);
            }
        }
    }

    private Bounds GetObjectBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        
        return bounds;
    }

    // Example usage to align multiple sets in a row
    public void AlignMultipleSets(Vector3 startPosition, int numLongTables, int numSmallTables)
    {
        Vector3 currentPosition = startPosition;

        // Place long tables
        for (int i = 0; i < numLongTables; i++)
        {
            AlignTableAndChairs(longTableSet, currentPosition);
            currentPosition.x += GetObjectBounds(longTableSet.tableObject).size.x + tableSpacing;
        }

        // Place small tables
        for (int i = 0; i < numSmallTables; i++)
        {
            AlignTableAndChairs(smallTableSet, currentPosition);
            currentPosition.x += GetObjectBounds(smallTableSet.tableObject).size.x + tableSpacing;
        }
    }
}
