using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] dialogue;

    public void Start()
    {
        Invoke("TriggerDialogue", 1f);
    }

    public void TriggerDialogue()
    {
        FindAnyObjectByType<DialogueManager>().StartMultipleDialogue(dialogue);
    }
}
