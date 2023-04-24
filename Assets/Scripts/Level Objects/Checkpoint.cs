using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private CheckpointManager checkpointManager = null;

    void Start()
    {
        checkpointManager = GameManager.Instance.GetCheckpointManager();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool player = other.gameObject.CompareTag("Player");

        if (player)
        {
            checkpointManager.SetCurrentCheckpoint(this);
        }
    }
}
