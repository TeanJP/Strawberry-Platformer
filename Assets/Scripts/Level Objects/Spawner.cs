using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private enum SpawnerState
    { 
        Idle,
        Activated
    }

    private enum HorizontalDirection
    {
        Left,
        Right
    }

    private SpawnerState spawnerState = SpawnerState.Activated;

    [SerializeField]
    private GameObject enemyToSpawn = null;
    private GameObject enemyInstance = null;

    [SerializeField]
    private float enemySpawnDelay = 5f;
    private float enemySpawnTimer = 0f;

    [SerializeField]
    private float spawnOffset = 0.01f;
    private float spawnerBase;

    [SerializeField]
    private bool patrol = false;
    [SerializeField]
    private HorizontalDirection initialDirection = HorizontalDirection.Left;

    void Start()
    {
        spawnerBase = GetComponent<SpriteRenderer>().bounds.min.y;
    }

    void Update()
    {
        switch(spawnerState)
        {
            case SpawnerState.Idle:
                if (enemyInstance == null)
                {
                    enemySpawnTimer = enemySpawnDelay;
                    spawnerState = SpawnerState.Activated;
                }
                break;
            case SpawnerState.Activated:
                if (enemySpawnTimer > 0f)
                {
                    enemySpawnTimer -= Time.deltaTime;
                }
                else
                {
                    enemyInstance = Instantiate(enemyToSpawn);
                    float enemyHeight = enemyInstance.GetComponent<SpriteRenderer>().bounds.size.y;
                    enemyInstance.transform.position = new Vector3(transform.position.x, spawnerBase + spawnOffset + enemyHeight * 0.5f, 0f);

                    if (initialDirection == HorizontalDirection.Right)
                    {
                        enemyInstance.transform.localScale *= new Vector2(-1f, 1f);
                    }

                    enemyInstance.GetComponent<Enemy>().SetPatrol(patrol);

                    spawnerState = SpawnerState.Idle;
                }
                break;
        }
    }
}
