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

    [SerializeField]
    private TextMeshProUGUI deathCountText = null;
    [SerializeField]
    private TextMeshProUGUI timeInLevelText = null;

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

    public void UpdateStatsDisplay()
    {
        deathCountText.text = scoreManager.GetDeathCount().ToString("n0");
        timeInLevelText.text = GetTimerText(scoreManager.GetTimeInLevel());
    }

    private string GetTimerText(float timerValue)
    {
        timerValue = Mathf.Max(timerValue, 0f);

        int minutesCount = 0;

        while (timerValue >= 60f)
        {
            timerValue -= 60f;
            minutesCount++;
        }

        string minutes = minutesCount.ToString();

        if (minutesCount < 10)
        {
            minutes = "0" + minutes;
        }

        int secondsCount = Mathf.FloorToInt(timerValue);
        string seconds = secondsCount.ToString();

        if (secondsCount < 10)
        {
            seconds = "0" + seconds;
        }

        timerValue -= secondsCount;

        int millisecondsCount = Mathf.CeilToInt(timerValue * 100f);
        string milliseconds = millisecondsCount.ToString();

        if (millisecondsCount < 10)
        {
            milliseconds = "0" + milliseconds;
        }
        else if (millisecondsCount >= 100)
        {
            milliseconds = milliseconds.Substring(0, 2);
        }

        return minutes + ":" + seconds + ":" + milliseconds;
    }
}
