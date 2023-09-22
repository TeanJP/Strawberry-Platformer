using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField]
    private List<Checkpoint> checkpoints = new List<Checkpoint>();

    private int currentCheckpoint = 0;

    private ScoreManager scoreManager = null;

    [SerializeField]
    private float spawnOffset = 0.01f;

    void Start()
    {
        scoreManager = GetComponent<ScoreManager>();
    }

    public void LoadCurrentCheckpoint()
    {
        if (PlayerPrefs.HasKey("Current Checkpoint"))
        {
            currentCheckpoint = PlayerPrefs.GetInt("Current Checkpoint");

            if (currentCheckpoint < 0 || currentCheckpoint >= checkpoints.Count)
            {
                currentCheckpoint = 0;
            }
        }
    }

    public void SaveCurrentCheckpoint()
    {
        PlayerPrefs.SetInt("Current Checkpoint", currentCheckpoint);
    }

    public Vector2 GetCurrentCheckpointPosition()
    {
        Checkpoint checkpoint = checkpoints[currentCheckpoint];

        float x = checkpoint.transform.position.x;
        float y = checkpoint.gameObject.GetComponent<SpriteRenderer>().bounds.min.y + spawnOffset;

        return new Vector2(x, y);
    }

    public void SetCurrentCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoints[currentCheckpoint] != checkpoint)
        {
            int checkpointIndex = checkpoints.IndexOf(checkpoint);

            if (checkpointIndex != -1)
            {
                currentCheckpoint = checkpointIndex;
            }

            scoreManager.SetCheckpointScore();
        }
    }

    public void ResetCurrentCheckpoint()
    {
        PlayerPrefs.SetInt("Current Checkpoint", 0);
    }
}
