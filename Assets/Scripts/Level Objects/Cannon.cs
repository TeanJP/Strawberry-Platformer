using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private enum State
    {
        Delayed,
        Loading,
        Charging
    }

    private State state;

    [SerializeField]
    private Vector2 direction = Vector2.left;
    [SerializeField]
    private float spawnOffset = 0.25f;

    private float timer;
    [SerializeField]
    private float loadDuration = 2f;
    [SerializeField]
    private float chargeDuration = 1f;

    [SerializeField]
    private float initialDelay = 0f;

    [SerializeField]
    private GameObject projectile = null;

    private LaunchedEnemy loadedProjectile = null;

    float height;

    void Start()
    {
        height = GetComponent<SpriteRenderer>().bounds.size.y; 
        
        if (initialDelay > 0f)
        {
            state = State.Delayed;
            timer = initialDelay;
        }
        else
        {
            state = State.Loading;
            timer = loadDuration;
        }

    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }

        switch (state)
        {
            case State.Delayed:
                if (timer <= 0f)
                {
                    state = State.Loading;
                    timer = loadDuration;
                }
                break;
            case State.Loading:
                if (timer <= 0f)
                {
                    loadedProjectile = Instantiate(projectile).GetComponent<LaunchedEnemy>();
                    loadedProjectile.SetDirection(direction, gameObject);

                    float sizeDifference = Mathf.Abs(loadedProjectile.GetComponent<BoxCollider2D>().bounds.size.y - height);

                    loadedProjectile.transform.position = (Vector2)transform.position + direction * (sizeDifference + spawnOffset);

                    state = State.Charging;
                    timer = chargeDuration;
                }
                break;
            case State.Charging:
                if (timer <= 0f)
                {
                    loadedProjectile.Activate();
                    state = State.Loading;
                    timer = loadDuration;
                }
                break;
        }
    }
}
