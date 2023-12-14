using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneForwarder : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;
    private bool started = true;

    private void Start()
    {
        StartCoroutine(WaitForTransition());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !started)
        {
            started = true;
            StartCoroutine(LoadLevel("Explanation Scene"));
        }
    }

    IEnumerator WaitForTransition()
    {
        yield return new WaitForSeconds(0.9f);
        started = false;
    }

    IEnumerator LoadLevel(string levelName)
    {
        transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(levelName);
    }
}
