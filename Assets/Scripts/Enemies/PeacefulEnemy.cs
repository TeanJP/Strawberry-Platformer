using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeacefulEnemy : Enemy
{
    private float fearSpreadDistance = 6f;

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        bool grounded = GetGrounded();

        UpdateState();

        bool hittingWall = GetHittingWall();
        bool dropAhead = GetDropAhead();

        switch (state)
        {
            case State.Default:
                if (patrol && grounded)
                {
                    Patrol(hittingWall, dropAhead);
                }
                break;
            case State.Scared:
                Run(hittingWall, dropAhead, Time.deltaTime);
                break;
            case State.Stunned:

                break;
        }

        UpdateGravityScale();
    }

    private void UpdateState()
    {
        float distanceFromPlayer;
        bool abovePlayer;
        bool playerSpeedScary;

        switch (state)
        {
            case State.Default:
                distanceFromPlayer = GetDistanceFromPlayer();
                abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y - 1f);
                playerSpeedScary = strawberry.GetSpeedAbovePercentage(fearSpeed);

                if (distanceFromPlayer < scaredDistance && !abovePlayer && playerSpeedScary)
                {
                    SetScared();
                }
                break;
            case State.Scared:
                distanceFromPlayer = GetDistanceFromPlayer();
                abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y - 1f);
                playerSpeedScary = strawberry.GetSpeedAbovePercentage(fearSpeed);

                if (distanceFromPlayer < scaredDistance && !abovePlayer && playerSpeedScary)
                {
                    fearTimer = fearDuration;
                }
                else
                {
                    DecrementFearTimer(Time.deltaTime);
                }
                break;
            case State.Stunned:
                DecrementStunTimer(Time.deltaTime);
                break;
        }
    }

    private void SpreadFear()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(activeCollider.bounds.center, fearSpreadDistance, enemyMask);

        for (int i = 0; i < enemies.Length; i++)
        {
            PeacefulEnemy peacefulEnemy = enemies[i].gameObject.GetComponent<PeacefulEnemy>();

            if (peacefulEnemy != null)
            {
                peacefulEnemy.SetScared();
            }
        }
    }

    protected override void SetDefeated()
    {
        SpreadFear();
        Destroy(gameObject);
    }
}
