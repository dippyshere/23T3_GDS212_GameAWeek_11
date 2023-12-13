using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorColour : MonoBehaviour
{
    public Material normalMaterial; // The default material
    public Material highlightMaterial; // The material to change to when the player steps on the tile

    private bool playerOnTile = false;
    private Renderer tileRenderer;

    void Start()
    {
        // Assuming the Renderer component is attached to the same GameObject
        tileRenderer = GetComponent<Renderer>();
        tileRenderer.material = normalMaterial;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTile = true;
            tileRenderer.material = highlightMaterial;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTile = false;
            tileRenderer.material = normalMaterial;
        }
    }

    void Update()
    {
        // You can add any additional logic here if needed
    }
}