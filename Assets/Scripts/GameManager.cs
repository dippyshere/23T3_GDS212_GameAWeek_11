using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject optionsPanel;
    public Animator transitionAnimator;
    public Animator transitionIconAnimator;

    public void Start()
    {

        // Ensure the options panel is initially inactive
        optionsPanel.SetActive(false);
    }

    public void StartIntro()
    {
        StartCoroutine(LoadLevel("IntroScene"));
    }

    public void StartGame()
    {
        // Load the game scene (change "GameScene" to your actual scene name)
        StartCoroutine(LoadLevel("Explanation Scene"));
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
        transitionIconAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(levelName);
    }
}