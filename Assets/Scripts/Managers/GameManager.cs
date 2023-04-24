using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{ 
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

    private CheckpointManager checkpointManager = null;

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

        checkpointManager = GetComponent<CheckpointManager>();
        checkpointManager.LoadCurrentCheckpoint();

        Vector2 spawnPosition = checkpointManager.GetCurrentCheckpointPosition();
        strawberryInstance = SpawnPlayer(spawnPosition);
    }

    void Update()
    {
        
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
}
