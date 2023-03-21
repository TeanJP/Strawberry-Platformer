using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeacefulEnemy : Enemy
{
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
                break;
            case State.Scared:
                if (distanceFromPlayer < scaredDistance)
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
}
