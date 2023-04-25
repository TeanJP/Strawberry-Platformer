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

        DecrementImmunityTimer(Time.deltaTime);
    }

    private void UpdateState()
    {
        bool scared;

        switch (state)
        {
            case State.Default:
                scared = GetScared();

                if (scared)
                {
                    SetScared();
                }
                break;
            case State.Scared:
                scared = GetScared();

                if (scared)
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

            if (peacefulEnemy != null && peacefulEnemy != this)
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
