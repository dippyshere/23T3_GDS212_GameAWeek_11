using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoryUtility : MonoBehaviour
{
    public GameObject[] scenes;

    private int sceneIndex = 0;

    private void Start()
    {
        
        for (int i = 0; i < scenes.Count(); i++)
        {
            HideScene(i);
        }
        
        ShowScene(0);
    }

    public void NextScene()
    {
        HideScene(sceneIndex);
        sceneIndex++;

        if (sceneIndex < scenes.Length)
        {
            ShowScene(sceneIndex);
        }
    }

    private void ShowScene(int index)
    {
        if (index >= 0 && index < scenes.Length)
        {
            scenes[index].SetActive(true);
        }
    }

    private void HideScene(int index)
    {
        if (index >= 0 && index < scenes.Length)
        {
            scenes[index].SetActive(false);
        }
    }
}