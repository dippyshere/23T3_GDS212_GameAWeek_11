using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject optionsPanel; // Reference to the options panel in the UI
    public Button startButton;      // Reference to the Start button in the UI
    public Button optionsButton;    // Reference to the Options button in the UI
    public Button quitButton;       // Reference to the Quit button in the UI
    public Animator transitionAnimator;

    public void Start()
    {
        // Add click event listeners to the buttons
        startButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);

        // Ensure the options panel is initially inactive
        optionsPanel.SetActive(false);
    }

    public void StartGame()
    {
        // Load the game scene (change "GameScene" to your actual scene name)
        StartCoroutine(LoadLevel("Explanation Scene"));
        startButton.interactable = false;
    }

    public void OpenOptions()
    {
        // Show the options panel
        optionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        // Quit the application (works in standalone builds, not in the Unity Editor)
        Application.Quit();
    }

    IEnumerator LoadLevel(string levelName)
    {
        transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(levelName);
    }
}