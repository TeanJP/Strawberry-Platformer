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
            //Load the player's current checkpoint.
            currentCheckpoint = PlayerPrefs.GetInt("Current Checkpoint");

            if (currentCheckpoint < 0 || currentCheckpoint >= checkpoints.Count)
            {
                //If the saved checkpoint is not valid for this level, set the current checkpoint as the first one.
                currentCheckpoint = 0;
            }

            checkpoints[currentCheckpoint].ActivateCheckpoint();
        }
    }

    public void SaveCurrentCheckpoint()
    {
        PlayerPrefs.SetInt("Current Checkpoint", currentCheckpoint);
    }

    public Vector2 GetCurrentCheckpointPosition()
    {
        //Get the coordinates of the bottom of the current checkpoint.
        Checkpoint checkpoint = checkpoints[currentCheckpoint];

        float x = checkpoint.transform.position.x;
        float y = checkpoint.gameObject.GetComponent<SpriteRenderer>().bounds.min.y + spawnOffset;

        return new Vector2(x, y);
    }

    public void SetCurrentCheckpoint(Checkpoint checkpoint)
    {
        //If the provided checkpoint is not the current checkpoint.
        if (checkpoints[currentCheckpoint] != checkpoint)
        {
            int checkpointIndex = checkpoints.IndexOf(checkpoint);

            if (checkpointIndex != -1)
            {
                //If the provided checkpoint is valid save it as the current checkpoint.
                currentCheckpoint = checkpointIndex;
            }

            //Save the player's current score.
            scoreManager.SetCheckpointScore();
        }
    }

    public void ResetCurrentCheckpoint()
    {
        //Set the current checkpoint as the first one.
        PlayerPrefs.SetInt("Current Checkpoint", 0);
    }
}
