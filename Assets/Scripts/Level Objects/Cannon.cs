using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private enum State
    {
        Loading,
        Charging
    }

    private State state = State.Loading;

    [SerializeField]
    private Vector2 direction = Vector2.left;

    private float timer;
    [SerializeField]
    private float loadDuration = 2f;
    [SerializeField]
    private float chargeDuration = 1f;

    [SerializeField]
    private GameObject projectile = null;

    private LaunchedEnemy loadedProjectile = null;

    void Start()
    {
        timer = loadDuration;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }

        switch (state)
        {
            case State.Loading:
                if (timer <= 0f)
                {
                    loadedProjectile = Instantiate(projectile).GetComponent<LaunchedEnemy>();
                    loadedProjectile.SetDirection(direction);

                    loadedProjectile.transform.position = new Vector2();

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
