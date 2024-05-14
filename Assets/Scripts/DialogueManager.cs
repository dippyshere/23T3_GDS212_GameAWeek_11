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
    public Image iconPortrait;
    public Button continueButton;
    public Volume focusPostProcess;

    public Animator animator;

    private Queue<string> sentences;
    private int sentenceIndex = 0;
    private int currentDialogueIndex = 0;
    private Dialogue currentDialogue;
    private Dialogue[] currentDialogues;

    private bool nextLoadQueued = false;

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
        iconPortrait.sprite = dialogue.icon;
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
        if (currentDialogue.dialogueAdvancedByPlacing.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByPlacing[sentenceIndex - 1] && animator.GetBool("DialogueActive"))
        {
            Debug.Log("Advanced by placing for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void AdvancePickup()
    {
        if (currentDialogue.dialogueAdvancedByPickingUp.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByPickingUp[sentenceIndex - 1] && animator.GetBool("DialogueActive"))
        {
            Debug.Log("Advanced by picking up for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void AdvanceRotate()
    {
        if (currentDialogue.dialogueAdvancedByRotating.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByRotating[sentenceIndex - 1] && animator.GetBool("DialogueActive"))
        {
            Debug.Log("Advanced by rotating for sentence index " + sentenceIndex);
            DisplayNextSentence();
        }
    }

    public void AdvanceCompletion()
    {
        if (currentDialogue.dialogueAdvancedByCompleting.Length > sentenceIndex - 1 && currentDialogue.dialogueAdvancedByCompleting[sentenceIndex - 1] && animator.GetBool("DialogueActive"))
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
            AudioManager.Instance.PlaySoundEffect(0);
        }
        if (sentenceIndex < currentDialogue.focusEnabled.Length && currentDialogue.focusEnabled[sentenceIndex])
        {
            BeginFocus();
        }
        else
        {
            EndFocus();
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
                PlayerController playerController = FindAnyObjectByType<PlayerController>();
                playerController.UnloadCurrentLevel();
            }
        }
        if (currentDialogue.dialogueLoadNextLevel != null)
        {
            if (sentenceIndex < currentDialogue.dialogueLoadNextLevel.Length && currentDialogue.dialogueLoadNextLevel[sentenceIndex] && !(currentDialogue.dialogueUnloadCurentLevel != null && sentenceIndex < currentDialogue.dialogueUnloadCurentLevel.Length && currentDialogue.dialogueUnloadCurentLevel[sentenceIndex]))
            {
                if (sentences.Count == 1)
                {
                    nextLoadQueued = true;
                }
                else
                {
                    PlayerController playerController = FindAnyObjectByType<PlayerController>();
                    playerController.LoadNextLevel();
                    nextLoadQueued = false;
                }
            }
        }
        sentenceIndex++;
        animator.SetBool("IconBounce", true);
        string sentence = sentences.Dequeue();
        StopCoroutine(nameof(TypeSentence));
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        int startingSentenceIndex = sentenceIndex;
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
            if (sentenceIndex != startingSentenceIndex)
            {
                break;
            }
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(0.016f);
        }
    }

    void EndDialogue()
    {
        if (currentDialogue.dialogueUnloadCurentLevel != null && sentenceIndex < currentDialogue.dialogueUnloadCurentLevel.Length && currentDialogue.dialogueUnloadCurentLevel[sentenceIndex])
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            playerController.UnloadCurrentLevel();
        }
        if (nextLoadQueued)
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            playerController.LoadNextLevel(true);
            nextLoadQueued = false;
        }
        if (currentDialogues!= null && currentDialogueIndex < currentDialogues.Length - 1)
        {
            currentDialogueIndex++;
            StartDialogue(currentDialogues[currentDialogueIndex]);
            AudioManager.Instance.PlaySoundEffect(0);
        }
        else
        {
            if (currentDialogues != null)
            {
                currentDialogues = null;
            }
            AudioManager.Instance.PlaySoundEffect(1);
            animator.SetBool("DialogueActive", false);
            continueButton.interactable = false;
            StartCoroutine(DisableFocus());
        }
    }

    private void BeginFocus()
    {
        StartCoroutine(EnableFocus());
    }

    private void EndFocus()
    {
        StartCoroutine(DisableFocus());
    }

    private IEnumerator EnableFocus()
    {
        StopCoroutine(DisableFocus());
        while (focusPostProcess.weight < 1f)
        {
            focusPostProcess.weight += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0.25f, Time.unscaledDeltaTime);
            yield return null;
        }
        focusPostProcess.weight = 1f;
        Time.timeScale = 0.075f;
        yield return null;
    }

    private IEnumerator DisableFocus()
    {
        StopCoroutine(EnableFocus());
        while (focusPostProcess.weight >= 0f)
        {
            focusPostProcess.weight -= Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.unscaledDeltaTime);
            yield return null;
        }
        focusPostProcess.weight = 0f;
        Time.timeScale = 1;
        yield return null;
    }
}
