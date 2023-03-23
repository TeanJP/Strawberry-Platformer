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
    [SerializeField]
    private GameObject shockwave = null;

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
                if (!startedJump)
                {
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
                        rb.velocity = new Vector2(0f, -bellyFlopStrength);
                        startedAttack = true;
                    }
                    else if (startedAttack)
                    {
                        PerformDownwardsAttack();
                    }
                }             
                break;
            case State.Scared:
                Run(hittingWall, dropAhead, Time.deltaTime);
                break;
            case State.Stunned:

                break;
        }

        UpdateGravityScale();

        if (state != State.Stunned)
        {
            DecrementAttackTimer(Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        bool isFloor = false;

        ContactPoint2D[] contacts = new ContactPoint2D[other.contactCount];
        other.GetContacts(contacts);

        for (int i = 0; i < contacts.Length; i++)
        {
            if (contacts[i].normal.y == 1f)
            {
                isFloor = true;
                break;
            }
        }

        if (state == State.Attacking && isFloor)
        {
            if (shockwave != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    Instantiate(shockwave, transform.position, transform.rotation);
                }
            }

            attackTimer = attackCooldown;
            startedJump = false;
            startedAttack = false;
        }
    }

    private void UpdateState()
    {
        float distanceFromPlayer = GetDistanceFromPlayer();

        switch (state)
        {
            case State.Default:              
                if (distanceFromPlayer < scaredDistance && attackTimer > 0f)
                {
                    SetScared();
                }
                else if (distanceFromPlayer < attackRange)
                {
                    state = State.Attacking;
                }
                break;
            case State.Attacking:
                if (distanceFromPlayer < scaredDistance && attackTimer > 0f)
                {
                    SetScared();
                }
                else if (distanceFromPlayer > attackRange)
                {
                    state = State.Default;
                }
                break;
            case State.Scared:
                if (trapped || attackTimer < 0f)
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

    private void PerformDownwardsAttack()
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x, activeCollider.bounds.min.y - attackCheckWidth * 0.5f);
        Vector2 boxSize = new Vector2(activeCollider.bounds.size.x, attackCheckWidth);

        Collider2D player = Physics2D.OverlapBox(boxPosition, boxSize, 0f, playerMask);

        if (player != null)
        {
            //Swap in actual values
            strawberry.TakeDamge(0, Vector2.one, 1f);
        }
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
        Destroy(gameObject);
    }
}
