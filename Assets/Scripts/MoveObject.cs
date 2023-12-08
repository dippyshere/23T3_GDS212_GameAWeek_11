using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    // Attach this script to the clickable object

    public int clickSoundIndex; // Index of the click sound effect

    private void OnMouseUp()
    {
        // This function is called when the object is clicked
        AudioManager.Instance.PlaySoundEffect(clickSoundIndex);
    }
}