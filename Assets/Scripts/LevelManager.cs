using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField, Tooltip("Compound required to complete the level")] public Compound requiredCompound;
    [SerializeField, Tooltip("Enable to ensure the background scene is loaded upon starting the game")] private bool loadBackgroundScene = true;
    [SerializeField] private Dialogue[] winDialogue;
    
    [Header("References")]
    [SerializeField, Tooltip("Reference to the Win Screen UI object (To enable upon completing the win condition)")] private GameObject winScreen;
    [SerializeField, Tooltip("Reference to the animator responsible for the level transitions (To trigger the animation to play when changing levels)")] public Animator transitionAnimator;
    [SerializeField, Tooltip("Reference to the animator responsible for the level transitions (To trigger the animation to play when changing levels)")] public Animator transitionIconAnimator;

    private PlayerController playerController;
    private bool hasWon = false;

    // private AsyncOperation levelLoad;

    private void Awake()
    {
        // check if the background scene is loaded
        if (SceneManager.GetSceneByName("Background Scene").isLoaded == false && loadBackgroundScene && !Application.isEditor)
        {
            // load the background scene on top of the current scene
            SceneManager.LoadSceneAsync("Background Scene", LoadSceneMode.Additive);
        }
    }

    // workaround for editor
    private void Start()
    {
        if (SceneManager.GetSceneByName("Background Scene").isLoaded == false && loadBackgroundScene && Application.isEditor)
        {
            // load the background scene on top of the current scene
            SceneManager.LoadSceneAsync("Background Scene", LoadSceneMode.Additive);
        }
        playerController = FindObjectOfType<PlayerController>();
    }

    //private void FixedUpdate()
    //{
    //    if (levelLoad != null && levelLoad.isDone)
    //    {
    //        levelLoad = null;
    //        // TODO: only start transition animation once complete
    //    }
    //}

    /// <summary>
    /// Checks if the win condition has been met
    /// </summary>
    public void CheckWinCondition()
    {
        if (requiredCompound.IsAssembled() && !hasWon)
        {
            Debug.Log("You win!");
            //winScreen.SetActive(true);
            hasWon = true;
            if (requiredCompound.name == Compound.CompoundType.None)
            {
                StartCoroutine(LoadLevel("Water Scene"));
                return;
            }
            FindObjectOfType<DialogueManager>().StartMultipleDialogue(winDialogue);
        }
    }

    /// <summary>
    /// Restarts the current level
    /// </summary>
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Loads the next level in the sequence, disables the button that called this function if provided, and begins the transition animation
    /// </summary>
    /// <param name="button">The button that called this function</param>
    public void NextLevel(Button button = null)
    {
        if (button != null)
        {
            button.interactable = false;
        }
        if (requiredCompound.name == Compound.CompoundType.None)
        {
            StartCoroutine(LoadLevel("Water Scene"));
            return;
        }
        playerController = FindObjectOfType<PlayerController>();
        playerController.LoadNextLevel();
        //switch (requiredCompound.name)
        //{
        //    case Compound.CompoundType.None:
        //        StartCoroutine(LoadLevel("Water Scene"));
        //        break;
        //    case Compound.CompoundType.Water:
        //        StartCoroutine(LoadLevel("Level 2"));
        //        break;
        //    case Compound.CompoundType.Methane:
        //        StartCoroutine(LoadLevel("Level 3"));
        //        break;
        //    case Compound.CompoundType.Ammonia:
        //        StartCoroutine(LoadLevel("Level 4"));
        //        break;
        //    case Compound.CompoundType.Methanol:
        //        StartCoroutine(LoadLevel("Level 5"));
        //        break;
        //    case Compound.CompoundType.AmmoniumHydroxide:
        //        StartCoroutine(LoadLevel("Level 6"));
        //        //SceneManager.LoadScene("Menu");
        //        break;
        //    case Compound.CompoundType.AcetateAcid:
        //        StartCoroutine(LoadLevel("Menu"));
        //        break;
        //}
    }

    /// <summary>
    /// Loads the level with the given name and begins the transition animation
    /// </summary>
    /// <param name="levelName">The name of the level to load</param>
    /// <returns></returns>
    public IEnumerator LoadLevel(string levelName)
    {
        transitionAnimator.SetTrigger("Start");
        transitionIconAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(levelName);
    }
}
