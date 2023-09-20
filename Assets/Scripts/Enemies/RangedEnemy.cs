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
    private Transform gunTransform = null;
    [SerializeField]
    private Vector2 gunBarrelOffset = new Vector2(0.5f, 0f);
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

        gunTransform = transform.GetChild(0).transform;

        currentAnimation = "Cowboy Enemy Idle";
    }

    protected override void Update()
    {
        base.Update();

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
                FacePlayer();

                if (currentShots > 0)
                {
                    if (attackTimer <= 0f)
                    {
                        CreateProjectile();

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

        ApplyAnimation(grounded);

        UpdateGravityScale();

        DecrementImmunityTimer(Time.deltaTime);
    }

    private void UpdateState()
    {
        float distanceFromPlayer;
        bool playerDefeated;
        bool scared;
        bool abovePlayer;

        switch (state)
        {
            case State.Default:
                playerDefeated = strawberry.GetDefeated();

                if (!playerDefeated)
                {
                    abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y);
                    distanceFromPlayer = GetDistanceFromPlayer();
                    scared = GetScared();

                    if (scared && currentShots == 0)
                    {
                        SetScared();
                    }
                    else if (distanceFromPlayer < attackRange && !abovePlayer)
                    {
                        state = State.Attacking;
                    }
                }
                break;
            case State.Attacking:
                playerDefeated = strawberry.GetDefeated();

                if (playerDefeated)
                {
                    state = State.Default;
                }
                else
                {
                    abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y);
                    distanceFromPlayer = GetDistanceFromPlayer();
                    scared = GetScared();

                    if (scared && currentShots == 0)
                    {
                        float facingDirection = GetFacingDirection();
                        float directionToPlayer = GetDirectionToPlayer();

                        if (trapped && facingDirection != directionToPlayer)
                        {
                            trapped = false;
                        }

                        if (!trapped)
                        {
                            SetScared();
                        }
                    }
                    else if (distanceFromPlayer > attackRange || abovePlayer)
                    {
                        state = State.Default;
                    }
                }
                break;
            case State.Scared:                    
                if (trapped)
                {
                    state = State.Attacking;
                }
                else
                {
                    scared = GetScared();

                    if (scared)
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

    private void CreateProjectile()
    {
        GameObject createdProjectile = Instantiate(projectile, (Vector2)gunTransform.position + gunBarrelOffset, Quaternion.identity);

        float horizontalDirection = GetFacingDirection();
        Vector2 projectileDirection = new Vector2(horizontalDirection, 0f);

        createdProjectile.GetComponent<EnemyProjectile>().SetDirection(projectileDirection);
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

    protected override void SetDefeated()
    {
        state = State.Defeated;
        activeCollider.enabled = false;

        gameManager.GetScoreManager().AddScore(score);
    }

    private void ApplyAnimation(bool grounded)
    {
        string animationToPlay = currentAnimation;

        switch (state)
        {
            case State.Default:
                if (grounded)
                {
                    if (rb.velocity.x != 0f)
                    {
                        animationToPlay = "Cowboy Enemy Moving";
                    }
                    else
                    {
                        animationToPlay = "Cowboy Enemy Idle";
                    }
                }
                else
                {
                    animationToPlay = "Cowboy Enemy Idle";
                }
                break;
            case State.Attacking:
                animationToPlay = "Cowboy Enemy Idle";
                break;
            case State.Scared:
                if (grounded)
                {
                    if (rb.velocity.x != 0f)
                    {
                        animationToPlay = "Cowboy Enemy Moving";
                    }
                    else
                    {
                        animationToPlay = "Cowboy Enemy Idle";
                    }
                }
                else
                {
                    animationToPlay = "Cowboy Enemy Idle";
                }
                break;
            case State.Stunned:
                animationToPlay = "Cowboy Enemy Idle";
                break;
            case State.Defeated:
                animationToPlay = "Cowboy Enemy Idle";
                break;
        }

        if (animationToPlay != currentAnimation)
        {
            animator.Play(animationToPlay);
            currentAnimation = animationToPlay;
        }
    }
}
