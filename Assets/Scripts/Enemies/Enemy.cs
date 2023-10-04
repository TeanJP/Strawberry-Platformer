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
        Alerted,
        Stunned,
        Defeated
    }

    protected enum FearLevel
    { 
        None,
        Low,
        High
    }

    protected State state = State.Default;
    [SerializeField]
    protected bool patrol = false;
    protected bool trapped = false;
    protected string currentAnimation;

    protected Rigidbody2D rb = null;
    protected SpriteRenderer spriteRenderer = null;
    protected BoxCollider2D activeCollider = null;
    protected Animator animator = null;

    protected GameManager gameManager = null;

    protected Strawberry strawberry = null;
    [SerializeField]
    protected float scaredDistance = 3f;
    [SerializeField]
    protected float fearDuration = 2f;
    protected float fearTimer = 0f;
    [SerializeField]
    protected float alertDuration = 0f;
    protected float alertTimer = 0f;
    [SerializeField]
    protected FearLevel fearLevel = FearLevel.High;

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
    protected float dropCheckOffset = 0.05f;
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
    [SerializeField]
    protected float lethalHitRepelStrength = 2f;

    protected float levelBoundary;

    [SerializeField]
    protected int score = 100;

    protected virtual void Start()
    {
        gameManager = GameManager.Instance;

        //Get the required components.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        activeCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        //Get the spacing of the raycasts for the wall and ground checks.
        verticalRaycastSpacing = activeCollider.bounds.size.x / (verticalRaycasts - 1);
        horizontalRaycastSpacing = activeCollider.bounds.size.y / (horizontalRaycasts - 1);

        strawberry = gameManager.GetStrawberryInstance();

        levelBoundary = gameManager.GetLevelBoundary();
    }

    protected virtual void Update()
    {
        //If the enemy fall of the bottom of the level set them as defeated.
        if (spriteRenderer.bounds.max.y < levelBoundary)
        {
            SetDefeated();
        }

        //If the enemy has been defeated and can't be seen destroy them.
        if (!spriteRenderer.isVisible && state == State.Defeated)
        {
            Destroy(gameObject);
        }
    }

    #region Movement
    protected void Patrol(bool hittingWall, bool dropAhead)
    {
        Vector2 movement = rb.velocity;

        bool enemyAhead = GetEnemyAhead();

        //Have the enemy turn around if it reaches an obstacle.
        if (hittingWall || dropAhead || enemyAhead)
        {
            FlipDirection();
        }

        //Have the enemy move in the direction it is facing.
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
            //If the enemy has space to run away from the player set them as no longer trapped.
            trapped = false;
        }
        else if (!trapped && (hittingWall || dropAhead))
        {
            //If there is an obstacle in the way of the enemy set them as trapped and stop them from moving. 
            trapped = true;
            currentSpeed = 0f;
            FacePlayer();
        }

        if (!trapped)
        {
            //Make the enemy face away from the player.
            if (facingDirection == directionToPlayer)
            {
                FlipDirection();
                currentSpeed = initialSpeed;
            }

            bool enemyAhead = GetEnemyAhead();

            if (enemyAhead)
            {
                //If there is another enemy in the way of this enemy stop it from moving.
                currentSpeed = 0f;
            }
            else
            {
                if (currentSpeed < initialSpeed)
                {
                    //If the enemy just started running set their speed to the initial run speed.
                    currentSpeed = initialSpeed;
                }
                else
                {
                    //Have the enemy accelerate to it's max speed.
                    currentSpeed = Mathf.Min(currentSpeed + acceleration * deltaTime, maxSpeed);
                }
            }
                
            movement.x = currentSpeed * GetFacingDirection();
        }
        else
        {
            //If the enemy is trapped stop them from moving.
            movement.x = 0f;
        }

        rb.velocity = movement;
    }

    protected void UpdateGravityScale()
    {
        //If the enemy is falling have them use a different gravity scale.
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
        return Mathf.Sign(transform.localScale.x) * -1f;
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

            //If a platform was hit return that there is a wall in front of the enemy.
            if (hit.collider != null)
            {
                return true;
            }

            //Adjust the position of the next raycast.
            raycastOrigin.y += horizontalRaycastSpacing;
        }

        return false;
    }

    protected bool GetDropAhead()
    {
        Vector2 raycastDirection = Vector2.down;
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + (activeCollider.bounds.extents.x + dropCheckOffset) * GetFacingDirection(), activeCollider.bounds.min.y);

        //Perform a raycast in front of the enemypointing down.
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

        if ( hit.collider != null)
        {
            //If a platform was hit that wasn't a spike return there as being no drop ahead.
            if (hit.collider.gameObject.GetComponent<Spike>())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            //If no platform was hit return there as being a drop ahead.
            return true;
        }
    }

    protected bool GetEnemyAhead()
    {
        Vector2 raycastDirection = new Vector2(GetFacingDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + activeCollider.bounds.extents.x * GetFacingDirection(), activeCollider.bounds.min.y);

        //Perform a raycast in front of the enemy pointing in the direction they are facing.
        RaycastHit2D[] hits = Physics2D.RaycastAll (raycastOrigin, raycastDirection, raycastLength, enemyMask);

        for (int i = 0; i < hits.Length; i++)
        {
            //If any enemies were hit by the raycast return there as being an enemy in the way.
            if (hits[i].collider != null && hits[i].collider.gameObject != gameObject)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Timers
    protected void DecrementStunTimer(float deltaTime)
    {
        if (stunTimer > 0f)
        {
            stunTimer -= deltaTime;

            //If the stun effect has expired set the enemy as no longer stunned.
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
                //If the enemies fear timer has expired set them as not scared and stop them from running.
                state = State.Default;
                rb.velocity = Vector2.zero;
                currentSpeed = 0f;
            }
        }
    }

    protected void DecrementImmunityTimer(float deltaTime)
    {
        //Decrement the immunity timer whilst there is time left on it.
        if (immunityTimer > 0f)
        {
            immunityTimer -= deltaTime;
        }
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(bool projectile, int damage, float stunDuration, Vector2 repelDirection, float repelStrength)
    {
        //If the attack that attempted to damage the enemy is a projectile or the enemy is not immune have them take damage.
        if (projectile || immunityTimer <= 0f)
        {
            health = Mathf.Max(health - damage, 0);

            if (health == 0)
            {
                //If the enemy has run out of health set them as defeated.
                SetDefeated();

                //Apply a stronger knockback if the knockback from the damage source is too weak.
                if (repelStrength < lethalHitRepelStrength)
                {
                    repelStrength = lethalHitRepelStrength;
                }
            }
            else
            {
                //If the damage source was not a projectile stun the enemy.
                if (!projectile)
                {
                    state = State.Stunned;
                    stunTimer = stunDuration;
                }
            }

            RepelEnemy(repelDirection, repelStrength);
            currentSpeed = 0f;

            //If the damage source was not a projectile make the enemy temporarily immune to damage.
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

        //Make the enemy face the player if they are not already facing them.
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

    protected bool GetScared()
    {
        bool scared = false;

        bool playerDefeated = strawberry.GetDefeated();

        if (fearLevel != FearLevel.None && !playerDefeated)
        {
            float distanceFromPlayer = GetDistanceFromPlayer();
            bool abovePlayer = strawberry.GetAbovePlayer(activeCollider.bounds.min.y);

            switch (fearLevel)
            {
                case FearLevel.Low:
                    bool playerRunning = strawberry.GetRunning();

                    if (distanceFromPlayer < scaredDistance && playerRunning && !abovePlayer)
                    {
                        scared = true;
                    }
                    break;
                case FearLevel.High:
                    if (distanceFromPlayer < scaredDistance && !abovePlayer)
                    {
                        scared = true;
                    }
                    break;
            }
        }

        return scared;
    }

    public float GetHorizontalVelocity()
    {
        return rb.velocity.x;
    }

    public void SetPatrol(bool patrol)
    {
        this.patrol = patrol;
    }

    public bool GetVisible()
    {
        return spriteRenderer.isVisible;
    }
}
