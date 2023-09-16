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
        if (!gameManager.GetEscapeActive())
        {
            bool player = other.gameObject.CompareTag("Player");

            if (player)
            {
                checkpointManager.SetCurrentCheckpoint(this);
            }
        }
    }
}
