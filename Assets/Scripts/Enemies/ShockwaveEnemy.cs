using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveEnemy : Enemy
{
    [SerializeField]
    private LayerMask playerMask;

    [SerializeField]
    private float attackRange = 6f;
    [SerializeField]
    private float attackCooldown = 3f;
    private float attackTimer = 0f;
    private bool startedJump = false;
    private bool startedAttack = false;

    float attackCheckWidth = 0.02f;
    [SerializeField]
    private float jumpStrength = 6f;
    [SerializeField]
    private float bellyFlopStrength = 10f;
    private int bellyFlopDamage = 10;
    private Vector2 bellyFlopRepelDirection = Vector2.one;
    private float bellyFlopRepelStrength = 3f;
    [SerializeField]
    private GameObject shockwave = null;

    protected override void Start()
    {
        base.Start();

        currentAnimation = "Shockwave Enemy Idle";
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
                if (!startedJump)
                {
                    FacePlayer();

                    //If the enemy can attack jump.
                    if (attackTimer <= 0f && grounded)
                    {
                        rb.velocity = new Vector2(0f, jumpStrength);
                        startedJump = true;
                    }
                }
                else
                {
                    if (!startedAttack && rb.velocity.y <= 0f)
                    {
                        //If the enemy hasn't already started going down, move downwards.
                        rb.velocity = new Vector2(0f, -bellyFlopStrength);
                        startedAttack = true;
                    }
                    else if (startedAttack)
                    {
                        //Whilst the enemy is moving down damage the player if they are hit.
                        PerformDownwardsAttack();
                    }
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

        if (state != State.Stunned)
        {
            DecrementAttackTimer(Time.deltaTime);
        }

        DecrementImmunityTimer(Time.deltaTime);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        bool platform = other.gameObject.CompareTag("Platform");
        bool breakable = other.gameObject.CompareTag("Breakable");

        if (platform || breakable)
        {
            bool isFloor = false;

            ContactPoint2D[] contacts = new ContactPoint2D[other.contactCount];
            other.GetContacts(contacts);

            for (int i = 0; i < contacts.Length; i++)
            {
                //Get whether the platform that was hit is below the enemy.
                if (contacts[i].normal.y == 1f)
                {
                    isFloor = true;
                    break;
                }
            }

            //If the enemy was attacking and hit the floor.
            if (state == State.Attacking && isFloor)
            {
                if (shockwave != null)
                {
                    Vector2 shockwaveDirection = Vector2.left;

                    //Create two shockwaves, with one going left and the other going right.
                    for (int i = 0; i < 2; i++)
                    {
                        CreateShockwave(shockwaveDirection);
                        shockwaveDirection *= -1f;
                    }
                }

                //Set the enemy as no longer attacking.
                attackTimer = attackCooldown;
                startedJump = false;
                startedAttack = false;
            }
        }
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

                    if (scared && attackTimer > 0f)
                    {
                        //If the enemy can't attack set it to be scared.
                        SetScared();
                    }
                    else if (distanceFromPlayer < attackRange && !abovePlayer)
                    {
                        //If the player is in range of the enemy set it to attack.
                        state = State.Alerted;
                        alertTimer = alertDuration;
                    }
                }
                break;
            case State.Attacking:
                if (attackTimer > 0f)
                {
                    playerDefeated = strawberry.GetDefeated();

                    //If the enemy can't attack yet and the player is alive check whether they should be set as scared.
                    if (!playerDefeated)
                    {
                        abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y);
                        distanceFromPlayer = GetDistanceFromPlayer();
                        scared = GetScared();

                        if (scared)
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
                    else
                    {
                        state = State.Default;
                    }
                }
                break;
            case State.Scared:
                if (trapped || attackTimer < 0f)
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

    private void PerformDownwardsAttack()
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x, activeCollider.bounds.min.y - attackCheckWidth * 0.5f);
        Vector2 boxSize = new Vector2(activeCollider.bounds.size.x, attackCheckWidth);

        //Check if the player is within a box below the enemy.
        Collider2D player = Physics2D.OverlapBox(boxPosition, boxSize, 0f, playerMask);

        if (player != null)
        {
            //If the player as hit damage them.
            float directionToPlayer = GetDirectionToPlayer();
            strawberry.TakeDamge(bellyFlopDamage, bellyFlopRepelDirection * new Vector2(directionToPlayer, 1f), bellyFlopRepelStrength);
        }
    }

    private void CreateShockwave(Vector2 direction)
    {
        //Instantiate the shockwave object and set it's position and direction.
        GameObject createdShockwave = Instantiate(shockwave);
        float shockwaveHeight = createdShockwave.GetComponent<SpriteRenderer>().bounds.extents.y;
        createdShockwave.transform.position = new Vector2(transform.position.x, activeCollider.bounds.min.y + shockwaveHeight);
        createdShockwave.GetComponent<EnemyProjectile>().SetDirection(direction);
    }

    private void DecrementAttackTimer(float deltaTime)
    {
        if (attackTimer > 0f)
        {
            attackTimer -= deltaTime;
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
                        animationToPlay = "Shockwave Enemy Moving";
                    }
                    else
                    {        
                        animationToPlay = "Shockwave Enemy Idle";
                    }
                }
                else
                {
                    animationToPlay = "Shockwave Enemy Idle";
                }
                break;
            case State.Attacking:
                animationToPlay = "Shockwave Enemy Idle";
                break;
            case State.Scared:
                if (grounded)
                {
                    if (rb.velocity.x != 0f)
                    {
                        animationToPlay = "Shockwave Enemy Moving";
                    }
                    else
                    {
                        animationToPlay = "Shockwave Enemy Idle";
                    }
                }
                else
                {
                    animationToPlay = "Shockwave Enemy Idle";
                }
                break;
            case State.Stunned:
                animationToPlay = "Shockwave Enemy Idle";
                break;
            case State.Defeated:
                animationToPlay = "Shockwave Enemy Idle";
                break;
        }

        if (animationToPlay != currentAnimation)
        {
            animator.Play(animationToPlay);
            currentAnimation = animationToPlay;
        }
    }
}
