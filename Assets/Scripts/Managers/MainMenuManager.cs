using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MenuManager
{
    [SerializeField]
    private string firstLevel = "Level One";

    public void NewGame()
    {
        //Set to play intro.
        //Reset level progress, not high scores.

        LoadLevel(firstLevel);
    }

    public void LoadLevel(string levelToLoad)
    {
        PlayerPrefs.SetInt("Current Checkpoint", 0);
        PlayerPrefs.SetInt("Checkpoint Score", 0);
        PlayerPrefs.SetInt("Death Count", 0);
        PlayerPrefs.SetFloat("Time in Level", 0f);
        LoadScene(levelToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
