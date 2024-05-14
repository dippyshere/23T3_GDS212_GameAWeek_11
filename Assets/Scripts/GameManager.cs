using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button startButton;
    public GameObject quitButton;
    public Animator transitionAnimator;
    public Animator transitionIconAnimator;

    public IEnumerator Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            quitButton.SetActive(false);
        }
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        transitionAnimator.SetTrigger("End");
        transitionIconAnimator.SetTrigger("End");
    }

    public void StartIntro()
    {
        startButton.interactable = false;
        StartCoroutine(LoadLevel("IntroScene"));
    }

    public void StartGame()
    {
        // Load the game scene (change "GameScene" to your actual scene name)
        startButton.interactable = false;
        StartCoroutine(LoadLevel("Explanation Scene"));
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