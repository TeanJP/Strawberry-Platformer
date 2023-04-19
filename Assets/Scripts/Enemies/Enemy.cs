using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected enum State
    {
        Default,
        Attacking,
        Scared,
        Stunned
    }

    protected State state = State.Default;
    [SerializeField]
    protected bool patrol = false;
    protected bool trapped = false;

    protected Rigidbody2D rb = null;
    protected BoxCollider2D activeCollider = null;

    [SerializeField]
    protected Strawberry strawberry = null;
    [SerializeField]
    protected float scaredDistance = 3f;
    [SerializeField]
    protected float fearSpeed = 1f;
    [SerializeField]
    protected float fearDuration = 2f;
    protected float fearTimer = 0f;

    #region Collision Checking
    [SerializeField]
    protected LayerMask platformMask;
    [SerializeField]
    protected LayerMask enemyMask;
    protected float raycastLeniency = 0.02f;
    protected float raycastLength = 0.02f;
    protected int horizontalRaycasts = 2;
    protected float horizontalRaycastSpacing;
    protected int verticalRaycasts = 3;
    protected float verticalRaycastSpacing;
    protected float dropCheckOffset = 0.03f;
    #endregion

    #region Movement Values
    [SerializeField]
    protected float maxSpeed = 8f;
    [SerializeField]
    protected float initialSpeed = 4f;
    protected float currentSpeed = 0f;
    [SerializeField]
    protected float acceleration = 6f;
    [SerializeField]
    protected float fallSpeed = 2.5f;
    #endregion

    [SerializeField]
    protected int health = 10;
    protected float stunTimer = 0f;
    [SerializeField]
    protected float immunityDuration = 1f;
    protected float immunityTimer = 0f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        activeCollider = GetComponent<BoxCollider2D>();

        verticalRaycastSpacing = activeCollider.bounds.size.x / (verticalRaycasts - 1);
        horizontalRaycastSpacing = activeCollider.bounds.size.y / (horizontalRaycasts - 1);
    }

    #region Movement
    protected void Patrol(bool hittingWall, bool dropAhead)
    {
        Vector2 movement = rb.velocity;

        bool enemyAhead = GetEnemyAhead();

        if (hittingWall || dropAhead || enemyAhead)
        {
            FlipDirection();
        }

        movement.x = initialSpeed * GetFacingDirection();

        rb.velocity = movement;
    }

    protected void Run(bool hittingWall, bool dropAhead, float deltaTime)
    {
        Vector2 movement = rb.velocity;

        float facingDirection = GetFacingDirection();
        float directionToPlayer = GetDirectionToPlayer();

        if (trapped && facingDirection != directionToPlayer)
        {
            trapped = false;
        }
        else if (!trapped && hittingWall || dropAhead)
        {
            trapped = true;
            currentSpeed = 0f;
            FacePlayer();
        }

        if (!trapped)
        {
            if (facingDirection == directionToPlayer)
            {
                FlipDirection();
                currentSpeed = initialSpeed;
            }

            bool enemyAhead = GetEnemyAhead();

            if (enemyAhead)
            {
                currentSpeed = 0f;
            }
            else
            {
                if (currentSpeed < initialSpeed)
                {
                    currentSpeed = initialSpeed;
                }
                else
                {
                    currentSpeed = Mathf.Min(currentSpeed + acceleration * deltaTime, maxSpeed);
                }
            }
                
            movement.x = currentSpeed * GetFacingDirection();
        }

        rb.velocity = movement;
    }

    protected void UpdateGravityScale()
    {
        if (rb.velocity.y < 0f)
        {
            rb.gravityScale = fallSpeed;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }
    #endregion

    #region Enemy Direction
    protected void FlipDirection()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

    protected float GetFacingDirection()
    {
        return Mathf.Sign(transform.localScale.x);
    }
    #endregion

    #region Colision Checking
    protected bool GetGrounded()
    {
        Vector2 raycastDirection = Vector2.down;
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.min.y);

        for (int i = 0; i < verticalRaycasts; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

            if (hit.collider != null)
            {
                return true;
            }

            raycastOrigin.x += verticalRaycastSpacing;
        }

        raycastOrigin.x = activeCollider.bounds.min.x - raycastLeniency;

        Vector2 wallCheckDirection = Vector2.left;
        Vector2 wallCheckOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.min.y);

        for (int i = 0; i < 2; i++)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, wallCheckDirection, raycastLength, platformMask);

            if (wallHit.collider == null)
            {
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

                if (hit.collider != null)
                {
                    return true;
                }
            }

            raycastOrigin.x += (activeCollider.bounds.size.x + raycastLeniency * 2f);
            wallCheckOrigin.x += activeCollider.bounds.size.x;
            wallCheckDirection *= -1f;
        }

        return false;
    }

    protected bool GetHittingWall()
    {
        Vector2 raycastDirection = new Vector2(GetFacingDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + activeCollider.bounds.extents.x * GetFacingDirection(), activeCollider.bounds.min.y);

        for (int i = 0; i < horizontalRaycasts; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

            if (hit.collider != null)
            {
                return true;
            }

            raycastOrigin.y += horizontalRaycastSpacing;
        }

        return false;
    }

    protected bool GetDropAhead()
    {
        Vector2 raycastDirection = new Vector2(GetFacingDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + (activeCollider.bounds.extents.x + dropCheckOffset) * GetFacingDirection(), activeCollider.bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

        return hit.collider == null;
    }

    protected bool GetEnemyAhead()
    {
        Vector2 raycastDirection = new Vector2(GetFacingDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + activeCollider.bounds.extents.x * GetFacingDirection(), activeCollider.bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, enemyMask);

        return hit.collider != null;
    }
    #endregion

    #region Timers
    protected void DecrementStunTimer(float deltaTime)
    {
        if (stunTimer > 0f)
        {
            stunTimer -= deltaTime;

            if (stunTimer <= 0f)
            {
                state = State.Default;
            }
        }
    }

    protected void DecrementFearTimer(float deltaTime)
    {
        if (fearTimer > 0f)
        {
            fearTimer -= deltaTime;

            if (fearTimer <= 0f)
            {
                state = State.Default;
            }
        }
    }

    protected void DecrementImmunityTimer(float deltaTime)
    {
        if (immunityTimer > 0f)
        {
            immunityTimer -= deltaTime;
        }
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(bool projectile, int damage, float stunDuration, Vector2 repelDirection, float repelStrength)
    {
        if (projectile || immunityTimer <= 0f)
        {
            health = Mathf.Max(health - damage, 0);

            if (health == 0)
            {
                SetDefeated();
            }
            else
            {
                state = State.Stunned;
                stunTimer = stunDuration;
            }

            RepelEnemy(repelDirection, repelStrength);

            if (!projectile)
            {
                immunityTimer = immunityDuration;
            }
        }
    }

    public void ApplyStun(float stunDuration)
    {
        stunTimer = stunDuration;
    }

    public void RepelEnemy(Vector2 repelDirection, float repelStrength)
    {
        rb.velocity = repelDirection * repelStrength;
    }

    protected abstract void SetDefeated();
    #endregion

    #region Direction to Player
    protected float GetDistanceFromPlayer()
    {
        Vector2 difference = strawberry.transform.position - transform.position;
        return difference.magnitude;
    }

    protected float GetDirectionToPlayer()
    {
        return Mathf.Sign(strawberry.transform.position.x - transform.position.x);
    }

    protected void FacePlayer()
    {
        bool facingPlayer = GetFacingPlayer();

        if (!facingPlayer)
        {
            FlipDirection();
        }
    }

    protected bool GetFacingPlayer()
    {
        float currentDirection = GetFacingDirection();
        float directionToPlayer = GetDirectionToPlayer();

        return currentDirection == directionToPlayer;
    }
    #endregion

    public void SetScared()
    {
        if (state != State.Scared)
        {
            state = State.Scared;
            fearTimer = fearDuration;
            currentSpeed = 0f;

            bool facingPlayer = GetFacingPlayer();

            if (facingPlayer)
            {
                FlipDirection();
            }
        }
    }

    public float GetHorizontalVelocity()
    {
        return rb.velocity.x;
    }
}
