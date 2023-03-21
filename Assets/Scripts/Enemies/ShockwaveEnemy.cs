using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveEnemy : Enemy
{
    private float attackRange = 6f;
    private float attackCooldown = 3f;
    private float attackTimer = 0f;

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
            case State.Attacking:

                break;
            case State.Scared:
                Run(hittingWall, dropAhead, Time.deltaTime);
                break;
            case State.Stunned:

                break;
        }
    }

    private void UpdateState()
    {
        float distanceFromPlayer = GetDistanceFromPlayer();

        switch (state)
        {
            case State.Default:              
                if (distanceFromPlayer < scaredDistance)
                {
                    state = State.Scared;

                    bool facingPlayer = GetFacingPlayer();

                    if (facingPlayer)
                    {
                        FlipDirection();
                        currentSpeed = 0f;
                    }
                }
                else if (distanceFromPlayer < attackRange)
                {

                }
                break;
            case State.Attacking:
                if (distanceFromPlayer < scaredDistance)
                {
                    state = State.Scared;

                    bool facingPlayer = GetFacingPlayer();

                    if (facingPlayer)
                    {
                        FlipDirection();
                        currentSpeed = 0f;
                    }
                }
                else if (distanceFromPlayer > attackRange)
                {
                    state = State.Default;
                }
                break;
            case State.Scared:
                if (trapped)
                {
                    state = State.Attacking;
                }
                else
                {
                    if (distanceFromPlayer < scaredDistance)
                    {
                        fearTimer = fearDuration;
                    }
                    else
                    {
                        DecrementFearTimer(Time.deltaTime);
                    }
                }
                break;
            case State.Stunned:
                DecrementStunTimer(Time.deltaTime);
                break;
        }
    }
}
