using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string name;
    public Sprite icon;
    public bool isIconOnLeft = false;
    [TextArea(3, 10)]
    public string[] sentences;
    public bool[] focusEnabled;
    public bool[] disableContinueButton;
    public bool[] dialogueAdvancedByPickingUp;
    public bool[] dialogueAdvancedByPlacing;
    public bool[] dialogueAdvancedByCompleting;
}
