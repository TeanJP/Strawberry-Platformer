using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{ 
    private enum GameState
    {
        Default,
        Escape,
        GameOver
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

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gamePaused = !gamePaused;
            pauseScreen.SetActive(gamePaused);

            if (gamePaused)
            {
                pauseMenuManager.ResetCurrentScreen();
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

                break;
            case GameState.GameOver:
                if (levelReloadTimer > 0f)
                {
                    levelReloadTimer -= Time.deltaTime;
                }
                else
                {
                    scoreManager.SaveCheckpointScore();
                    checkpointManager.SaveCurrentCheckpoint();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                break;
        }
    }

    private Strawberry SpawnPlayer(Vector2 spawnPosition)
    {
        GameObject createdPlayer = Instantiate(strawberry);

        float halfHeight = 0f;

        BoxCollider2D[] colliders = createdPlayer.GetComponents<BoxCollider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].enabled)
            {
                halfHeight = colliders[i].bounds.extents.y;
                break;
            }
        }

        Vector2 offset = new Vector2(0f, halfHeight);

        createdPlayer.transform.position = spawnPosition + offset;
        createdPlayer.transform.rotation = Quaternion.identity;

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
}
