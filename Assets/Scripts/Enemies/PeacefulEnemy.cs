using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeacefulEnemy : Enemy
{
    private float fearSpreadDistance = 6f;

    protected override void Start()
    {
        base.Start();

        currentAnimation = "Default Enemy Idle";
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
        bool playerDefeated;
        bool scared;

        switch (state)
        {
            case State.Default:
                playerDefeated = strawberry.GetDefeated();

                if (!playerDefeated)
                {
                    scared = GetScared();

                    if (scared)
                    {
                        //Set the enemy as alerted.
                        state = State.Alerted;
                        alertTimer = alertDuration;
                    }
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
            case State.Alerted:
                if (alertTimer > 0f)
                {
                    alertTimer -= Time.deltaTime;
                }
                else
                {
                    //If the enemy is no longer alerted set them as scared.
                    SetScared();
                }
                break;
            case State.Stunned:
                DecrementStunTimer(Time.deltaTime);
                break;
        }
    }

    private void SpreadFear()
    {
        //Get all nearby enemies.
        Collider2D[] enemies = Physics2D.OverlapCircleAll(activeCollider.bounds.center, fearSpreadDistance, enemyMask);

        for (int i = 0; i < enemies.Length; i++)
        {
            PeacefulEnemy peacefulEnemy = enemies[i].gameObject.GetComponent<PeacefulEnemy>();

            //Set all of the peaceful enemies that were found as scared.
            if (peacefulEnemy != null && peacefulEnemy != this)
            {
                peacefulEnemy.SetScared();
            }
        }
    }

    protected override void SetDefeated()
    {
        SpreadFear();
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
                        animationToPlay = "Default Enemy Moving";
                    }
                    else
                    {
                        animationToPlay = "Default Enemy Idle";
                    }
                }
                else
                {
                    animationToPlay = "Default Enemy Idle";
                }
                break;
            case State.Scared:
                if (grounded)
                {
                    if (rb.velocity.x != 0f)
                    {
                        animationToPlay = "Default Enemy Moving";
                    }
                    else
                    {
                        animationToPlay = "Default Enemy Idle";
                    }
                }
                else
                {
                    animationToPlay = "Default Enemy Idle";
                }
                break;
            case State.Stunned:
                animationToPlay = "Default Enemy Idle";
                break;
            case State.Defeated:
                animationToPlay = "Default Enemy Idle";
                break;
        }

        if (animationToPlay != currentAnimation)
        {
            animator.Play(animationToPlay);
            currentAnimation = animationToPlay;
        }
    }
}
