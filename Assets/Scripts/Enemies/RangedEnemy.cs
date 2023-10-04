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
                            //If the enemy ran out of shots set it to reload.
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
            case State.Alerted:
                FacePlayer();
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
                        //If the enemy should be scared of the player and can't attack set it to be scared.
                        SetScared();
                    }
                    else if (distanceFromPlayer < attackRange && !abovePlayer)
                    {
                        //If the player is within range and can be seen by the enemy set the enemy to start attacking.
                        state = State.Alerted;
                        alertTimer = alertDuration;
                    }
                }
                break;
            case State.Attacking:
                playerDefeated = strawberry.GetDefeated();

                if (playerDefeated)
                {
                    //If the player was defeated stop attacking.
                    state = State.Default;
                }
                else
                {
                    abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y);
                    distanceFromPlayer = GetDistanceFromPlayer();
                    scared = GetScared();

                    //If the enemy should be scared of the player and can't attack set it to be scared.
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
                        //If the player goes out of range and sight of the enemy stop them from attacking.
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
            case State.Alerted:
                if (alertTimer > 0f)
                {
                    alertTimer -= Time.deltaTime;
                }
                else
                {
                    state = State.Attacking;
                }
                break;
            case State.Stunned:
                DecrementStunTimer(Time.deltaTime);
                break;
        }
    }

    private void CreateProjectile()
    {
        //Create a projectile and set it's direction and position based on that of the enemy.
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

            //If the enemy has completed it's reload, give it more shots.
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
