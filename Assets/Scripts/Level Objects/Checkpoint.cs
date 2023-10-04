using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private GameManager gameManager = null;
    private CheckpointManager checkpointManager = null;

    void Start()
    {
        gameManager = GameManager.Instance;
        checkpointManager = gameManager.GetCheckpointManager();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //If the game is not in the escape phase.
        if (!gameManager.GetEscapeActive())
        {
            bool player = other.gameObject.CompareTag("Player");

            if (player)
            {
                //If the player collided with the checkpoint set the current checkpoint to this checkpoint.
                checkpointManager.SetCurrentCheckpoint(this);
            }
        }
    }
}
