using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawGrid : MonoBehaviour
{
    [SerializeField] private float gridSize;
    [SerializeField] private int gridCount;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int x = 0; x < gridCount; x++)
        {
            for (int y = 0; y < gridCount; y++)
            {
                Vector3 pos = new Vector3(x * gridSize, 4, y * gridSize);
                Gizmos.DrawWireCube(pos, new Vector3(gridSize, 0, gridSize));
            }
        }
    }
}
