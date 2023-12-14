using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image iconLeft;
    public Image iconRight;
    public Button continueButton;
    public Volume focusPostProcess;

    public Animator animator;

    private Queue<string> sentences;
    private int sentenceIndex = 0;
    private int currentDialogueIndex = 0;
    private Dialogue currentDialogue;
    private Dialogue[] currentDialogues;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartMultipleDialogue(Dialogue[] dialogues)
    {
        currentDialogueIndex = 0;
        currentDialogues = dialogues;
        StartDialogue(dialogues[currentDialogueIndex]);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        sentenceIndex = 0;
        currentDialogue = dialogue;
        animator.SetBool("DialogueActive", true);
        nameText.text = dialogue.name;
        continueButton.interactable = true;
        if (dialogue.isIconOnLeft)
        {
            iconLeft.sprite = dialogue.icon;
            iconLeft.enabled = true;
            iconRight.enabled = false;
        }
        else
        {
            iconRight.sprite = dialogue.icon;
            iconRight.enabled = true;
            iconLeft.enabled = false;
        }
        //if (dialogue.focusEnabled.Length > 0 && dialogue.focusEnabled[0])
        //{
        //    StartCoroutine(EnableFocus());
        //}
        //else
        //{
        //    StartCoroutine(DisableFocus());
        //}
        if (dialogue.disableContinueButton.Length > 0 && dialogue.disableContinueButton[0])
        {
            continueButton.interactable = false;
        }
        else
        {
            continueButton.interactable = true;
        }
        if (sentences == null)
        {
            sentences = new Queue<string>();
        }
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void AdvancePlacement()
    {
        if (currentDialogue.dialogueAdvancedByPlacing.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByPlacing[sentenceIndex - 1])
        {
            Debug.Log("Advanced by placing for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void AdvancePickup()
    {
        if (currentDialogue.dialogueAdvancedByPickingUp.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByPickingUp[sentenceIndex - 1])
        {
            Debug.Log("Advanced by picking up for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void AdvanceRotate()
    {
        if (currentDialogue.dialogueAdvancedByRotating.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByRotating[sentenceIndex - 1])
        {
            Debug.Log("Advanced by rotating for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void AdvanceCompletion()
    {
        if (currentDialogue.dialogueAdvancedByCompleting.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByCompleting[sentenceIndex - 1])
        {
            Debug.Log("Advanced by completing for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        if (sentenceIndex > 0)
        {
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySoundEffect(0);
            }
        }
        if (sentenceIndex < currentDialogue.focusEnabled.Length && currentDialogue.focusEnabled[sentenceIndex])
        {
            StartCoroutine(EnableFocus());
        }
        else
        {
            StartCoroutine(DisableFocus());
        }
        if (sentenceIndex < currentDialogue.disableContinueButton.Length && currentDialogue.disableContinueButton[sentenceIndex])
        {
            continueButton.interactable = false;
        }
        else
        {
            continueButton.interactable = true;
        }
        if (currentDialogue.dialogueUnloadCurentLevel != null)
        {
            if (sentenceIndex < currentDialogue.dialogueUnloadCurentLevel.Length && currentDialogue.dialogueUnloadCurentLevel[sentenceIndex])
            {
                PlayerController playerController = FindObjectOfType<PlayerController>();
                playerController.LoadNextLevel();
            }
        }
        sentenceIndex++;
        animator.SetBool("IconBounce", true);
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        char[] chars = sentence.ToCharArray();
        string[] sentenceArray = new string[chars.Length];
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '<')
            {
                sentenceArray[i] = sentence.Substring(i, sentence.IndexOf('>', i) - i + 1);
                i = sentence.IndexOf('>', i);
            }
            else
            {
                sentenceArray[i] = chars[i].ToString();
            }
        }
        sentenceArray = new List<string>(sentenceArray).FindAll(s => !string.IsNullOrEmpty(s)).ToArray();
        foreach (string letter in sentenceArray)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(0.016f);
        }
    }

    void EndDialogue()
    {
        if (sentenceIndex < currentDialogue.dialogueUnloadCurentLevel.Length && currentDialogue.dialogueUnloadCurentLevel[sentenceIndex])
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            playerController.LoadNextLevel();
        }
        if (currentDialogues!= null && currentDialogueIndex < currentDialogues.Length - 1)
        {
            currentDialogueIndex++;
            StartDialogue(currentDialogues[currentDialogueIndex]);
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySoundEffect(0);
            }
        }
        else
        {
            if (currentDialogues != null)
            {
                currentDialogues = null;
            }
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySoundEffect(1);
            }
            animator.SetBool("DialogueActive", false);
            continueButton.interactable = false;
            StartCoroutine(DisableFocus());
        }
    }

    private IEnumerator EnableFocus()
    {
        StopCoroutine(DisableFocus());
        focusPostProcess.weight = 1;
        Time.timeScale = 0.25f;
        yield return null;
        //float t = 0;
        //while (t < 1)
        //{
        //    t += Time.unscaledDeltaTime * 2;
        //    focusPostProcess.weight = Mathf.Lerp(0, 1, t);
        //    Time.timeScale = Mathf.Lerp(1, 0.5f, t);
        //    yield return null;
        //}
    }

    private IEnumerator DisableFocus()
    {
        StopCoroutine(EnableFocus());
        focusPostProcess.weight = 0;
        Time.timeScale = 1f;
        yield return null;
        //float t = 0;
        //while (t < 1)
        //{
        //    t += Time.unscaledDeltaTime * 2;
        //    focusPostProcess.weight = Mathf.Lerp(1, 0, t);
        //    Time.timeScale = Mathf.Lerp(0.5f, 1, t);
        //    yield return null;
        //}
    }
}
