using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MenuManager
{
    private CheckpointManager checkpointManager = null;
    private ScoreManager scoreManager = null;

    private GameObject startScreen = null;

    void Start()
    {
        checkpointManager = GetComponent<CheckpointManager>();
        scoreManager = GetComponent<ScoreManager>();

        startScreen = currentScreen;
    }

    public void LoadCheckpoint()
    {
        //Include penalties for loading checkpoint, void the no death bonus.
        scoreManager.SaveCheckpointScore();
        checkpointManager.SaveCurrentCheckpoint();
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartLevel()
    {
        scoreManager.ResetCheckpointScore();
        checkpointManager.ResetCurrentCheckpoint();
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetCurrentScreen()
    {
        if (currentScreen != startScreen)
        {
            OpenScreen(startScreen);
        }
    }
}
