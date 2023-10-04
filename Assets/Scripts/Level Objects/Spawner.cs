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
                    //If the enemy that was spawned no longer exists set the spawner to spawn another.
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
                    //Spawn an enemy and store it.
                    enemyInstance = Instantiate(enemyToSpawn);
                    //Adjust the enemy position to be just above the ground.
                    float enemyHeight = enemyInstance.GetComponent<SpriteRenderer>().bounds.size.y;
                    enemyInstance.transform.position = new Vector3(transform.position.x, spawnerBase + spawnOffset + enemyHeight * 0.5f, 0f);

                    //Set the initial direction of the enemy.
                    if (initialDirection == HorizontalDirection.Right)
                    {
                        enemyInstance.transform.localScale *= new Vector2(-1f, 1f);
                    }

                    //Set whether the enemy should patrol.
                    enemyInstance.GetComponent<Enemy>().SetPatrol(patrol);

                    //Set the spawner as no longer having to spawn an enemy.
                    spawnerState = SpawnerState.Idle;
                }
                break;
        }
    }
}
