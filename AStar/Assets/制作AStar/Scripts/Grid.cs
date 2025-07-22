using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    private Node[,] grid;
    
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPoint)
    {
        float percentageX = (worldPoint.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentageY = (worldPoint.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentageX = Mathf.Clamp01(percentageX);
        percentageY = Mathf.Clamp01(percentageY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentageX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentageY);
        return grid[x, y];
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));
        if (grid != null)
        {
            Node playerNode = NodeFromWorldPoint(player.position);
            foreach (Node node in grid)
            {
                Gizmos.color = node.walkable ? Color.white : Color.red;
                if (playerNode == node)
                {
                    Gizmos.color = Color.cyan;
                }
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
