using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
        GameManager gameManager = GameManager.Instance;
        gameManager.ResumeGame();
        gameManager.GetStrawberryInstance().SetDefeated();
    }

    public void RestartLevel()
    {
        scoreManager.ResetCheckpointScore();
        scoreManager.ResetDeathCount();
        scoreManager.ResetTimeInLevel();
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
