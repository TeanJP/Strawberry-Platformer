using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField]
    private float attackRange = 8f;
    [SerializeField]
    private float attackCooldown = 1f;
    private float attackTimer = 0f;

    [SerializeField]
    private GameObject projectile = null;
    [SerializeField]
    private float reloadDuration = 4f;
    private float reloadTimer = 0f;
    [SerializeField]
    private int maxShots = 6;
    private int currentShots;

    protected override void Start()
    {
        base.Start();

        currentShots = maxShots;
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
                if (currentShots > 0)
                {
                    if (attackTimer <= 0f)
                    {
                        Instantiate(projectile);
                        currentShots--;

                        if (currentShots == 0)
                        {
                            reloadTimer = reloadDuration;
                        }
                        else
                        {
                            attackTimer = attackCooldown;
                        }
                    }
                    else
                    {
                        DecrementAttackTimer(Time.deltaTime);
                    }
                }
                else
                {
                    DecrementReloadTimer(Time.deltaTime);
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
                    state = State.Attacking;
                }
                break;
            case State.Attacking:
                if (distanceFromPlayer < scaredDistance && currentShots == 0)
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

    private void DecrementAttackTimer(float deltaTime)
    {
        if (attackTimer > 0f)
        {
            attackTimer -= deltaTime;
        }
    }

    private void DecrementReloadTimer(float deltaTime)
    {
        if (reloadTimer > 0f)
        {
            reloadTimer -= deltaTime;

            if (reloadTimer <= 0f)
            {
                currentShots = maxShots;
            }
        }
    }
}
