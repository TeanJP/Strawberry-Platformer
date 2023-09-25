using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{ 
    private enum GameState
    {
        Default,
        Escape,
        GameOver,
        GameWon
    }

    private enum HorizontalDirection
    {
        Left,
        Right
    }

    private GameState gameState = GameState.Default;

    private bool gamePaused = false;

    private static GameManager gameManagerInstance;

    public static GameManager Instance
    {
        get
        {
            return gameManagerInstance;
        }
    }

    [SerializeField]
    private GameObject strawberry = null;
    [SerializeField]
    private HorizontalDirection initialDirection = HorizontalDirection.Right;
    private Strawberry strawberryInstance = null;

    private ScoreManager scoreManager = null;
    private CheckpointManager checkpointManager = null;
    [SerializeField]
    private CameraBehaviour cameraBehaviour = null;

    [SerializeField]
    private float levelReloadDelay = 3f;
    private float levelReloadTimer = 0f;

    [SerializeField]
    private GameObject lecheEnergyDisplay = null;
    [SerializeField]
    private GameObject strawberryHeartsDisplay = null;
    [SerializeField]
    private GameObject pauseScreen = null;
    private PauseMenuManager pauseMenuManager = null;

    [SerializeField]
    private TextMeshProUGUI escapeTimerText = null;
    [SerializeField]
    private float escapeTimeLimit = 30f;
    private float escapeTimer;

    [SerializeField]
    private LevelExit levelExit = null;

    [SerializeField]
    private GameObject defaultToggleBlocks = null;
    [SerializeField]
    private GameObject escapeToggleBlocks = null;

    void Awake()
    {      
        if (gameManagerInstance != null && gameManagerInstance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            gameManagerInstance = this;
        }

        scoreManager = GetComponent<ScoreManager>();

        checkpointManager = GetComponent<CheckpointManager>();
        checkpointManager.LoadCurrentCheckpoint();

        Vector2 spawnPosition = checkpointManager.GetCurrentCheckpointPosition();
        strawberryInstance = SpawnPlayer(spawnPosition);

        pauseMenuManager = GetComponent<PauseMenuManager>();
        pauseScreen.SetActive(gamePaused);

        escapeTimerText.gameObject.SetActive(false);

        if (defaultToggleBlocks != null)
        {
            defaultToggleBlocks.SetActive(true);
        }

        if (escapeToggleBlocks != null)
        {
            escapeToggleBlocks.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameState != GameState.GameOver && gameState != GameState.GameWon)
        {
            gamePaused = !gamePaused;
            pauseScreen.SetActive(gamePaused);

            if (gamePaused)
            {
                pauseMenuManager.ResetCurrentScreen();
                pauseMenuManager.UpdateStatsDisplay();
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        switch (gameState)
        {
            case GameState.Default:

                break;
            case GameState.Escape:
                if (escapeTimer > 0f)
                {
                    escapeTimer -= Time.deltaTime;
                    escapeTimerText.text = GetTimerText(escapeTimer);
                }
                else
                {
                    strawberryInstance.SetDefeated();
                }
                break;
            case GameState.GameOver:
                if (levelReloadTimer > 0f)
                {
                    levelReloadTimer -= Time.deltaTime;
                }
                else
                {
                    scoreManager.SaveCheckpointScore();
                    scoreManager.IncrementDeathCount();
                    scoreManager.SaveTimeInLevel();
                    checkpointManager.SaveCurrentCheckpoint();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                break;
            case GameState.GameWon:

                break;
        }
    }

    private Strawberry SpawnPlayer(Vector2 spawnPosition)
    {
        GameObject createdPlayer = Instantiate(strawberry);

        float halfHeight = 0f;
        float offset = 0f;

        BoxCollider2D[] colliders = createdPlayer.GetComponents<BoxCollider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].enabled)
            {
                halfHeight = colliders[i].bounds.extents.y;
                offset = colliders[i].offset.y;
                break;
            }
        }

        Vector2 spawnOffset = new Vector2(0f, halfHeight + offset * -1f);

        createdPlayer.transform.position = spawnPosition + spawnOffset;
        createdPlayer.transform.rotation = Quaternion.identity;

        if (initialDirection == HorizontalDirection.Right)
        {
            createdPlayer.transform.localScale *= new Vector2(-1f, 1f);
        }

        return createdPlayer.GetComponent<Strawberry>();
    }

    public Strawberry GetStrawberryInstance()
    {
        return strawberryInstance;
    }

    public CheckpointManager GetCheckpointManager()
    {
        return checkpointManager;
    }

    public void SetGameOver()
    {
        gameState = GameState.GameOver;
        
        levelReloadTimer = levelReloadDelay;
        
        cameraBehaviour.enabled = false;
    }

    public ScoreManager GetScoreManager()
    {
        return scoreManager;
    }

    public GameObject GetLecheEnergyDisplay()
    {
        return lecheEnergyDisplay;
    }

    public GameObject GetStrawberryHeartsDisplay()
    {
        return strawberryHeartsDisplay;
    }

    public bool GetGamePaused()
    {
        return gamePaused;
    }

    public void ResumeGame()
    {
        if (gamePaused)
        {
            gamePaused = false;
            pauseScreen.SetActive(gamePaused);
            Time.timeScale = 1f;
        }
    }

    public void StartEscape()
    {
        gameState = GameState.Escape;

        escapeTimer = escapeTimeLimit;
        escapeTimerText.gameObject.SetActive(true);
        escapeTimerText.text = GetTimerText(escapeTimer);

        levelExit.SetOpen();

        if (defaultToggleBlocks != null)
        {
            defaultToggleBlocks.SetActive(false);
        }

        if (escapeToggleBlocks != null)
        {
            escapeToggleBlocks.SetActive(true);
        }
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

    public bool GetEscapeActive()
    {
        return gameState == GameState.Escape;
    }

    public void SetGameWon()
    {
        gameState = GameState.GameWon;

        scoreManager.EndCombo();
        scoreManager.OpenScoreScreen();
    }

    public bool GetGameWon()
    {
        return gameState == GameState.GameWon;
    }

    public float GetEscapeTimeLimit()
    {
        return escapeTimeLimit;
    }

    public float GetEscapeTimeRemaining()
    {
        return escapeTimer;
    }

    public string GetActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public float GetLevelBoundary()
    {
        return cameraBehaviour.GetLevelBoundary();
    }
}
