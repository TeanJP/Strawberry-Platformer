using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strawberry : MonoBehaviour
{
    #region State Enums
    private enum MovementState
    {
        Default,
        Running,
        Crawling,
        BellyFlopping,
        Stunned
    }

    private enum RunState
    {
        Default,
        Rolling,
        Turning,
        Stopping,
        WallRunning,
        WallJumping,
        Diving,
        SuperJumping,
        ChargingSuperJump,
        CancellingSuperJump
    }
    #endregion

    #region States
    private MovementState movementState = MovementState.Default;
    private RunState runState = RunState.Default;

    private bool grounded = false;

    private bool movementApplied = false;
    #endregion

    #region Collision Checking
    [Header("Collision Checking")]
    [SerializeField]
    private LayerMask platformMask;
    [SerializeField]
    private LayerMask enemyMask;
    private float attackCheckWidth = 0.04f;
    private float raycastLeniency = 0.02f;
    private float raycastLength = 0.02f;
    private float fullColliderHeight;
    private int horizontalRaycasts = 3;
    private float horizontalRaycastSpacing;
    private int verticalRaycasts = 3;
    private float verticalRaycastSpacing;
    #endregion

    #region Components
    private Rigidbody2D rb = null;
    private SpriteRenderer spriteRenderer = null;
    private BoxCollider2D activeCollider = null;
    [Header("Components")]
    [SerializeField]
    private BoxCollider2D fullCollider = null;
    [SerializeField]
    private BoxCollider2D halfCollider = null;
    [SerializeField]
    private Sprite fullSprite = null;
    [SerializeField]
    private Sprite halfSprite = null;
    #endregion

    #region Inputs
    private float horizontalInput = 0f;
    private bool releasedJumpInput = false;
    private bool runInput = false;
    private bool upInput = false;
    private bool downInput = false;
    #endregion

    #region Buffers
    [Header("Buffers")]
    [SerializeField]
    private float jumpBuffer = 0.2f;
    private float jumpTimer = 0f;

    [SerializeField]
    private float groundedBuffer = 0.15f;
    private float groundedTimer = 0f;

    [SerializeField]
    private float boostBuffer = 1f;
    private float boostTimer = 0f;
    #endregion

    #region Movement Values
    private float currentSpeed = 0f;
    private float previousSpeed;

    [Header("Movement Values")]
    [SerializeField]
    private float initialWalkSpeed = 2f;
    [SerializeField]
    private float maxWalkSpeed = 5f;
    [SerializeField]
    private float walkAcceleration = 6f;

    [SerializeField]
    private float initialRunSpeed = 5f;
    [SerializeField]
    private float maxRunSpeed = 10f;
    [SerializeField]
    private float runAcceleration = 5f;

    [SerializeField]
    private float maxSpeed = 30f;
    [SerializeField]
    private float speedReduction = 1f;

    [SerializeField]
    private float jumpStrength = 6f;
    [SerializeField]
    private float fallSpeed = 2.5f;
    [SerializeField]
    private float incompleteJumpStrength = 0.25f;

    [SerializeField]
    private float crawlSpeed = 3f;
    [SerializeField]
    private float crawlJumpStrength = 6f;

    [SerializeField]
    private float bellyFlopStrength = 10f;
    [SerializeField]
    private float flopRecoveryDuration = 0.5f;
    private float flopRecoveryTimer = 0f;

    [SerializeField]
    private float turnDeceleration = 6f;

    [SerializeField]
    private float slideDeceleration = 6f;

    [SerializeField]
    private float diveSpeed = 10f;
    [SerializeField]
    private Vector2 diveDirection = new Vector2(1f, -1f);

    [SerializeField]
    private float wallRunSpeed = 6f;

    [SerializeField]
    private float wallJumpStrength = 6f;
    [SerializeField]
    private Vector2 wallJumpDirection = new Vector2(1f ,1f);

    [SerializeField]
    private float superJumpSpeed = 10f;
    [SerializeField]
    private float superJumpChargeSpeed = 3f;

    [SerializeField]
    private float superJumpCancelSpeed = 10f;
    [SerializeField]
    private Vector2 superJumpCancelDirection = new Vector2(1f, 0f);

    [SerializeField]
    private float collisionRepelStrength = 3f;
    [SerializeField]
    private Vector2 collisionRepelDirection = new Vector2(1f, 1f);
    #endregion

    #region Health
    [Header("Health Values")]
    [SerializeField]
    private int maxHearts = 100;
    private int hearts = 1;

    [SerializeField]
    private float stunDuration = 0.5f;
    private float stunTimer = 0f;

    [SerializeField]
    private float invincibilityDuratrion = 3f;
    private float invincibilityTimer = 0f;

    private GameObject heart = null;
    #endregion

    #region Attack Values
    [SerializeField]
    private float attackStunDuration = 2f;
    private int fullDamage = 100;
    [SerializeField]
    private float fullRepelStrength = 4f;
    [SerializeField]
    private int reducedDamage = 5;
    [SerializeField]
    private float reducedRepelStrength = 2f;
    [SerializeField]
    private float minimumSpeed = 8f;
    [SerializeField]
    private int minimumDamage = 1;
    [SerializeField]
    private float minimumRepelStrength = 1f;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = fullSprite;

        fullColliderHeight = fullCollider.bounds.extents.y * 2f;

        activeCollider = fullCollider;

        Vector2 activeColliderDimensions = activeCollider.bounds.size;

        verticalRaycastSpacing = activeColliderDimensions.x / (verticalRaycasts - 1);
        horizontalRaycastSpacing = activeColliderDimensions.y / (horizontalRaycasts - 1);

        diveDirection.Normalize();
        wallJumpDirection.Normalize();
        superJumpCancelDirection.Normalize();
        collisionRepelDirection.Normalize();
    }

    void Update()
    {
        grounded = GetGrounded();
        
        if (grounded)
        {
            groundedTimer = groundedBuffer;
        }
        
        bool hittingWall = GetHittingWall();
        bool hittingCeiling = GetHittingCeiling();

        GetInputs();
        ApplyInputs(grounded, hittingWall, hittingCeiling);
        Move(grounded);
        Attack();
        DecrementTimers(Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            bool applyStun = false, isWall = false, isFloor = false, isCeiling = false;

            ContactPoint2D[] contacts = new ContactPoint2D[other.contactCount];
            other.GetContacts(contacts);

            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].normal.x == GetPlayerDirection() * -1f)
                {
                    isWall = true;
                }
                else if (contacts[i].normal.y == 1f)
                {
                    isFloor = true;
                }
                else if (contacts[i].normal.y == -1f)
                {
                    isCeiling = true;
                }
            }

            if (movementState == MovementState.Running)
            {
                if (((runState == RunState.Default && grounded) || runState == RunState.Stopping) && isWall)
                {
                    applyStun = true;
                }
                else if ((runState == RunState.Rolling || runState == RunState.Diving) && isWall)
                {
                    applyStun = true;
                    SwapActiveCollider();
                }
                else if ((runState == RunState.WallRunning || runState == RunState.SuperJumping) && isCeiling)
                {
                    movementState = MovementState.Default;
                    runState = RunState.Default;
                    rb.velocity = Vector2.zero;
                }
            }
            else if (movementState == MovementState.BellyFlopping && isFloor)
            {
                flopRecoveryTimer = flopRecoveryDuration;
                rb.velocity = Vector2.zero;
            }

            if (applyStun)
            {
                ApplyStun();
                RepelPlayer(collisionRepelDirection * new Vector2(GetPlayerDirection() * -1f, 1f), collisionRepelStrength);
            }
        }
    }

    #region Movement
    private void GetInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Z))
        {
            jumpTimer = jumpBuffer;

            releasedJumpInput = false;
        }
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            releasedJumpInput = true;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            runInput = true;
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            runInput = false;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            downInput = true;
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            downInput = false;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            upInput = true;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            upInput = false;
        }
    }

    private void ApplyInputs(bool grounded, bool hittingWall, bool hittingCeiling)
    {
        switch (movementState)
        {
            case MovementState.Default:
                if (GetFacingIncorrectDirection())
                {
                    FlipPlayerDirection();
                }

                if (grounded && !hittingWall && runInput)
                {
                    movementState = MovementState.Running;
                    runState = RunState.Default;
                }
                else if (downInput)
                {
                    if (grounded)
                    {
                        movementState = MovementState.Crawling;
                    }
                    else
                    {
                        movementState = MovementState.BellyFlopping;
                        movementApplied = false;
                    }

                    SwapActiveCollider();
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (!runInput && boostTimer <= 0f)
                        {
                            runState = RunState.Stopping;
                        }
                        else
                        {
                            if (downInput)
                            {
                                if (grounded)
                                {
                                    runState = RunState.Rolling;
                                }
                                else
                                {
                                    runState = RunState.Diving;
                                    boostTimer = 0f;
                                }

                                SwapActiveCollider();
                            }
                            else if (!grounded && hittingWall)
                            {
                                runState = RunState.WallRunning;
                                currentSpeed = wallRunSpeed;
                                boostTimer = 0f;
                            }
                            else if (upInput && grounded && currentSpeed >= maxRunSpeed)
                            {
                                runState = RunState.ChargingSuperJump;
                                SwapActiveCollider();
                                boostTimer = 0f;
                            }
                            else if (GetFacingIncorrectDirection() && grounded)
                            {
                                FlipPlayerDirection();
                                runState = RunState.Turning;
                                previousSpeed = currentSpeed;
                                boostTimer = 0f;
                            }
                        }                    
                        break;
                    case RunState.Rolling:
                        if (!downInput && !hittingCeiling)
                        {
                            if (runInput)
                            {
                                runState = RunState.Default;
                            }
                            else if (boostTimer <= 0f)
                            {
                                runState = RunState.Stopping;
                            }

                            SwapActiveCollider();
                        }
                        else if (downInput && !grounded)
                        {
                            runState = RunState.Diving;
                            boostTimer = 0f;
                        }
                        break;
                    case RunState.WallRunning:
                        if (runInput)
                        {
                            if (hittingWall)
                            {
                                if (jumpTimer > 0f)
                                {
                                    FlipPlayerDirection();                                  
                                    runState = RunState.WallJumping;
                                    
                                    movementApplied = false;
                                    jumpTimer = 0f;
                                }
                            }
                            else
                            {
                                runState = RunState.Default;

                                rb.velocity *= new Vector2(1f, 0f);
                            }
                        }
                        else
                        {
                            movementState = MovementState.Default;
                            runState = RunState.Default;

                            rb.velocity *= new Vector2(1f, 0f);
                        }
                        break;
                    case RunState.WallJumping:
                        if (grounded)
                        {
                            if (runInput)
                            {
                                runState = RunState.Default;
                            }
                            else
                            {
                                runState = RunState.Stopping;
                            }
                        }
                        else if (hittingWall)
                        {
                            if (runInput)
                            {
                                runState = RunState.WallRunning;
                            }
                            else
                            {
                                movementState = MovementState.Default;
                                runState = RunState.Default;
                            }
                        }
                        else if (downInput)
                        {
                            runState = RunState.Diving;
                            SwapActiveCollider();
                        }
                        break;
                    case RunState.Diving:
                        if (jumpTimer > 0f)
                        {
                            movementState = MovementState.BellyFlopping;
                            runState = RunState.Default;
                            
                            movementApplied = false;
                            jumpTimer = 0f;
                        }
                        else if (grounded && !hittingWall)
                        {
                            if (runInput)
                            {
                                if (downInput)
                                {
                                    runState = RunState.Rolling;
                                }
                                else
                                {
                                    runState = RunState.Default;
                                    SwapActiveCollider();
                                }
                            }
                            else
                            {
                                runState = RunState.Stopping;
                                SwapActiveCollider();
                            }
                        }
                        break;
                    case RunState.SuperJumping:
                        if (jumpTimer > 0f)
                        {
                            if (GetFacingIncorrectDirection())
                            {
                                FlipPlayerDirection();
                            }

                            runState = RunState.CancellingSuperJump;
                            
                            movementApplied = false;
                            jumpTimer = 0f;
                        }
                        break;
                    case RunState.ChargingSuperJump:
                        if (!upInput)
                        {
                            runState = RunState.SuperJumping;
                            SwapActiveCollider();
                        }
                        break;
                    case RunState.CancellingSuperJump:
                        if (hittingWall && runInput)
                        {
                            runState = RunState.WallRunning;
                            currentSpeed = wallRunSpeed;
                        }
                        else if (grounded)
                        {
                            if (runInput)
                            {
                                runState = RunState.Default;
                                currentSpeed = maxRunSpeed;
                            }
                            else
                            {
                                runState = RunState.Stopping;
                            }
                        }
                        break;
                }
                break;
            case MovementState.Crawling:
                if (GetFacingIncorrectDirection())
                {
                    FlipPlayerDirection();
                }

                if (!downInput && !hittingCeiling && grounded)
                {
                    movementState = MovementState.Default;
                    SwapActiveCollider();
                }
                break;
            case MovementState.BellyFlopping:
                if (!grounded && flopRecoveryTimer > 0f)
                {
                    if (downInput)
                    {
                        movementState = MovementState.Crawling;
                    }
                    else
                    {
                        movementState = MovementState.Default;
                        SwapActiveCollider();
                    }

                    movementApplied = false;
                    flopRecoveryTimer = 0f;
                }
                break;
        }
    }

    private void Move(bool grounded)
    {
        float horizontalDirection = GetPlayerDirection();

        Vector2 movement = rb.velocity;

        switch (movementState)
        {
            case MovementState.Default:
                if (horizontalInput == 0f)
                {
                    currentSpeed = 0f;
                }
                else
                {
                    if (currentSpeed == 0f)
                    {
                        currentSpeed = initialWalkSpeed;
                    }
                    else
                    {
                        currentSpeed = Mathf.Min(currentSpeed + walkAcceleration * Time.deltaTime, maxWalkSpeed);
                    }
                }

                movement.x = horizontalDirection * currentSpeed;

                if (jumpTimer > 0f && groundedTimer > 0f && movement.y <= 0f)
                {
                    movement.y = jumpStrength;
                    jumpTimer = 0f;
                }

                if (grounded)
                {
                    movementApplied = false;
                }

                if (movement.y > 0f && releasedJumpInput && !movementApplied)
                {
                    movement.y *= incompleteJumpStrength;
                    movementApplied = true;
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (grounded)
                        {
                            if (currentSpeed < initialRunSpeed)
                            {
                                currentSpeed = initialRunSpeed;
                            }
                            else if (currentSpeed < maxRunSpeed)
                            {
                                currentSpeed = Mathf.Min(currentSpeed + runAcceleration * Time.deltaTime, maxRunSpeed);
                            }
                        }

                        movement.x = horizontalDirection * currentSpeed;

                        if (jumpTimer > 0f && groundedTimer > 0f && movement.y <= 0f)
                        {
                            movement.y = jumpStrength;
                            jumpTimer = 0f;
                        }

                        if (grounded)
                        {
                            movementApplied = false;
                        }

                        if (movement.y > 0f && releasedJumpInput && !movementApplied)
                        {
                            movement.y *= incompleteJumpStrength;
                            movementApplied = true;
                        }
                        break;
                    case RunState.Rolling:
                        movement.x = horizontalDirection * currentSpeed;
                        break;
                    case RunState.Turning:
                        currentSpeed = Mathf.Max(currentSpeed - turnDeceleration * Time.deltaTime, 0f);

                        movement.x = horizontalDirection * -1f * currentSpeed;

                        if (currentSpeed == 0f && grounded)
                        {
                            runState = RunState.Default;
                            currentSpeed = previousSpeed;
                        }
                        break;
                    case RunState.Stopping:
                        currentSpeed = Mathf.Max(currentSpeed - slideDeceleration * Time.deltaTime, 0f);

                        movement.x = horizontalDirection * currentSpeed;

                        if (currentSpeed == 0f)
                        {
                            movementState = MovementState.Default;
                            runState = RunState.Default;
                        }
                        break;
                    case RunState.WallRunning:
                        movement = new Vector2(0f, currentSpeed);
                        break;
                    case RunState.WallJumping:
                        if (!movementApplied)
                        {
                            movement = new Vector2(horizontalDirection, 1f) * wallJumpDirection * wallJumpStrength;
                            movementApplied = true;
                        }
                        break;
                    case RunState.Diving:
                        movement = diveDirection * new Vector2(horizontalDirection, 1f) * diveSpeed;
                        break;
                    case RunState.SuperJumping:
                        movement = new Vector2(0f, superJumpSpeed);
                        break;
                    case RunState.ChargingSuperJump:
                        movement.x = horizontalInput * superJumpChargeSpeed;
                        break;
                    case RunState.CancellingSuperJump:
                        if (!movementApplied)
                        {
                            movement.y = 0f;
                            movementApplied = true;
                        }

                        movement.x = horizontalDirection * superJumpCancelSpeed;
                        break;
                }
                break;
            case MovementState.Crawling:

                if (horizontalInput != 0f)
                {
                    movement.x = horizontalDirection * crawlSpeed;
                }
                else
                {
                    movement.x = 0f;
                }

                if (jumpTimer > 0f && groundedTimer > 0f && movement.y <= 0f)
                {
                    movement.y = crawlJumpStrength;
                    jumpTimer = 0f;
                }

                if (grounded)
                {
                    movementApplied = false;
                }

                if (movement.y > 0f && releasedJumpInput && !movementApplied)
                {
                    movement.y *= incompleteJumpStrength;
                    movementApplied = true;
                }
                break;
            case MovementState.BellyFlopping:               
                if (!movementApplied)
                {
                    movement = new Vector2(0f, -bellyFlopStrength);
                }              
                break;
        }
        
        if (movementState == MovementState.Running && (runState == RunState.Diving || runState == RunState.WallRunning || runState == RunState.SuperJumping))
        {
            rb.gravityScale = 0f;
        }
        else
        {
            if (movement.y < 0f && !(movementState == MovementState.Running && runState == RunState.CancellingSuperJump))
            {
                rb.gravityScale = fallSpeed;
            }
            else
            {
                rb.gravityScale = 1f;
            }
        }
        
        if (currentSpeed > maxRunSpeed)
        {
            currentSpeed = Mathf.Max(currentSpeed - speedReduction * Time.deltaTime, maxRunSpeed);
        }

        rb.velocity = movement;
    }
    #endregion

    #region Player Direction
    private bool GetFacingIncorrectDirection()
    {
        return horizontalInput != 0f && horizontalInput != Mathf.Sign(transform.localScale.x);
    }

    private void FlipPlayerDirection()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

    public float GetPlayerDirection()
    {
        return Mathf.Sign(transform.localScale.x);
    }
    #endregion

    private void DecrementTimers(float deltaTime)
    {
        if (jumpTimer > 0f)
        {
            jumpTimer -= deltaTime;
        }

        if (groundedTimer > 0f)
        {
            groundedTimer -= deltaTime;
        }

        if (boostTimer > 0f)
        {
            boostTimer -= deltaTime;
        }

        if (stunTimer > 0f)
        {
            stunTimer -= deltaTime;

            if (stunTimer <= 0f)
            {
                movementState = MovementState.Default;
            }
        }

        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= deltaTime;
        }

        if (flopRecoveryTimer > 0f)
        {
            flopRecoveryTimer -= deltaTime;

            if (flopRecoveryTimer <= 0f)
            {
                if (downInput)
                {
                    movementState = MovementState.Crawling;
                }
                else
                {
                    movementState = MovementState.Default;
                    SwapActiveCollider();
                }

                movementApplied = false;
            }
        }
    }

    #region Collision Checks
    private bool GetGrounded()
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

    private bool GetHittingWall()
    {
        Vector2 raycastDirection = new Vector2(GetPlayerDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + activeCollider.bounds.extents.x * GetPlayerDirection(), activeCollider.bounds.min.y);

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
        
    private bool GetHittingCeiling()
    {
        Vector2 raycastDirection = Vector2.up;
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.min.y + fullColliderHeight);

        for (int i = 0; i < verticalRaycasts;  i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

            if (hit.collider != null)
            {
                return true;
            }

            raycastOrigin.x += verticalRaycastSpacing;
        }

        return false;
    }
    #endregion

    #region Attacks
    private void Attack()
    {
        float facingDirection = GetPlayerDirection();
        bool falling = rb.velocity.y <= 0f;

        switch (movementState)
        {
            case MovementState.Default:
                if (!grounded && falling)
                {
                    PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (currentSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(facingDirection, fullDamage, fullRepelStrength);
                        }
                        else if (currentSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(facingDirection, reducedDamage, reducedRepelStrength);
                        }
                        else
                        {
                            PerformHorizontalAttack(facingDirection, minimumDamage, minimumRepelStrength);
                        }

                        if (!grounded && falling)
                        {
                            PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                        }
                        break;
                    case RunState.Rolling:
                        if (currentSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(facingDirection, fullDamage, fullRepelStrength);
                        }
                        else if (currentSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(facingDirection, reducedDamage, reducedRepelStrength);
                        }
                        else
                        {
                            PerformHorizontalAttack(facingDirection, minimumDamage, minimumRepelStrength);
                        }
                        break;
                    case RunState.Turning:
                        if (previousSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(facingDirection * -1f, fullDamage, fullRepelStrength);
                        }
                        else if (previousSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(facingDirection * -1f, reducedDamage, reducedRepelStrength);
                        }
                        else
                        {
                            PerformHorizontalAttack(facingDirection * -1f, minimumDamage, minimumRepelStrength);
                        }

                        if (!grounded && falling)
                        {
                            PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                        }
                        break;
                    case RunState.Stopping:
                        if (currentSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(facingDirection, fullDamage, fullRepelStrength);
                        }
                        else if (currentSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(facingDirection, reducedDamage, reducedRepelStrength);
                        }
                        else
                        {
                            PerformHorizontalAttack(facingDirection, minimumDamage, minimumRepelStrength);
                        }
                        if (!grounded && falling)
                        {
                            PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                        }
                        break;
                    case RunState.WallRunning:
                        PerformUpwardsAttack(reducedDamage, reducedRepelStrength);
                        break;
                    case RunState.WallJumping:
                        PerformHorizontalAttack(facingDirection, reducedDamage, reducedRepelStrength);
                        PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                        break;
                    case RunState.Diving:
                        PerformHorizontalAttack(facingDirection, reducedDamage, reducedRepelStrength);
                        PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                        break;
                    case RunState.SuperJumping:
                        PerformUpwardsAttack(fullDamage, fullRepelStrength);
                        break;
                    case RunState.CancellingSuperJump:
                        PerformHorizontalAttack(facingDirection, fullDamage, fullRepelStrength);
                        PerformDownwardsAttack(fullDamage, fullRepelStrength);
                        break;
                }
                break;
            case MovementState.Crawling:
                if (!grounded && falling)
                {
                    PerformDownwardsAttack(reducedDamage, reducedRepelStrength);
                }
                break;
            case MovementState.BellyFlopping:
                if (flopRecoveryTimer <= 0f)
                {
                    PerformDownwardsAttack(fullDamage, fullRepelStrength);
                }
                break;
        }
    }


    private void PerformHorizontalAttack(float horizontalDirection, int damage, float repelStrength)
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x + (activeCollider.bounds.extents.x + attackCheckWidth * 0.5f) * horizontalDirection, activeCollider.bounds.center.y);
        Vector2 boxSize = new Vector2(attackCheckWidth, activeCollider.bounds.size.y);

        Collider2D[] enemies = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, enemyMask);

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i].GetComponent<Enemy>();

            if (enemy != null)
            {
                Vector2 repelDirection = new Vector2(horizontalDirection, 1f);
                repelDirection.Normalize();
                enemy.TakeDamage(false, damage, attackStunDuration, repelDirection, repelStrength);
            }
        }
    }
    
    private void PerformDownwardsAttack(int damage, float repelStrength)
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x, activeCollider.bounds.min.y - attackCheckWidth * 0.5f);
        Vector2 boxSize = new Vector2(activeCollider.bounds.size.x, attackCheckWidth);

        Collider2D[] enemies = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, enemyMask);

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i].GetComponent<Enemy>();

            if (enemy != null)
            {
                Vector2 repelDirection = new Vector2(Mathf.Sign(enemy.transform.position.x - boxPosition.x), 1f);
                repelDirection.Normalize();
                enemy.TakeDamage(false, damage, attackStunDuration, repelDirection, repelStrength);
            }
        }
    }

    private void PerformUpwardsAttack(int damage, float repelStrength)
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x, activeCollider.bounds.max.y + attackCheckWidth * 0.5f);
        Vector2 boxSize = new Vector2(activeCollider.bounds.size.x, attackCheckWidth);

        Collider2D[] enemies = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, enemyMask);

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i].GetComponent<Enemy>();

            if (enemy != null)
            {
                Vector2 repelDirection = new Vector2(Mathf.Sign(enemy.transform.position.x - boxPosition.x), 1f);
                repelDirection.Normalize();
                enemy.TakeDamage(false, damage, attackStunDuration, repelDirection, repelStrength);
            }
        }
    }
    #endregion

    #region Taking Damage
    public void TakeDamge(int damage, Vector2 repelDirection, float repelStrength)
    {
        if (invincibilityTimer <= 0f)
        {
            if (hearts == 0 || damage > maxHearts)
            {
                Debug.Log("GAME OVER");
            }
            else
            {
                int previousHearts = hearts;
                hearts = Mathf.Max(hearts - damage, 0);

                SpawnHearts(previousHearts - hearts);

                invincibilityTimer = invincibilityDuratrion;
                ApplyStun();
            }

            RepelPlayer(repelDirection, repelStrength);
        }
    }

    private void SpawnHearts(int amount)
    {
        /*

        for (int i = 0; i < amount; i++)
        {
            Instantiate(heart);
        }
        */
    }

    public void ApplyStun()
    {
        movementState = MovementState.Stunned;
        runState = RunState.Default;
        stunTimer = stunDuration;
        movementApplied = false;
        currentSpeed = 0f;
    }

    public void RepelPlayer(Vector2 repelDirection, float repelStrength)
    {
        rb.velocity = repelDirection * repelStrength;
    }
    #endregion

    private void SwapActiveCollider()
    {
        fullCollider.enabled = !fullCollider.enabled;
        halfCollider.enabled = !halfCollider.enabled;

        if (halfCollider.enabled)
        {
            spriteRenderer.sprite = halfSprite;
            activeCollider = halfCollider;
        }
        else
        {
            spriteRenderer.sprite = fullSprite;
            activeCollider = fullCollider;
        }
        
        horizontalRaycastSpacing = activeCollider.bounds.size.y / (horizontalRaycasts - 1);
    }

    #region Enemy Interaction
    public bool GetSpeedAbovePercentage(float percentage)
    {
        float minimum = Mathf.Lerp(initialRunSpeed, maxRunSpeed, percentage);

        if (currentSpeed >= minimum && movementState == MovementState.Running)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool GetAbovePlayer(float position)
    {
        if (activeCollider.bounds.max.y < position)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Leche Interaction
    public float GetVerticalVelocity()
    {
        return rb.velocity.y;
    }

    public Vector2 GetCentre()
    {
        return activeCollider.bounds.center;
    }

    public bool GetStunned()
    {
        return movementState == MovementState.Stunned;
    }

    public bool GetWallRunning()
    {
        return movementState == MovementState.Running && runState == RunState.WallRunning;
    }

    public bool GetUsingHalfCollider()
    {
        return activeCollider == halfCollider;
    }
    #endregion

    #region Adding Speed
    public bool GetCanAddSpeed()
    {
        bool validState = false;

        switch (movementState)
        {
            case MovementState.Default:
                validState = true;
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        validState = true;
                        break;
                    case RunState.Rolling:
                        validState = true;
                        break;
                    case RunState.Turning:
                        validState = true;
                        break;
                    case RunState.Stopping:
                        validState = true;
                        break;
                }
                break;
            case MovementState.Crawling:
                validState = true;
                break;
        }

        return grounded && validState;
    }

    public void AddSpeed(float speedDirection, float speed)
    {
        currentSpeed = Mathf.Min(currentSpeed + speed, maxSpeed);

        float playerDirection = GetPlayerDirection();

        if (playerDirection != speedDirection)
        {
            FlipPlayerDirection();
        }

        boostTimer = boostBuffer;

        movementState = MovementState.Running;

        switch (movementState)
        {
            case MovementState.Default:
                runState = RunState.Default;
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Turning:
                        runState = RunState.Default;
                        break;
                    case RunState.Stopping:
                        runState = RunState.Default;
                        break;
                }
                break;
            case MovementState.Crawling:
                runState = RunState.Rolling;
                break;
        }       
    }
    #endregion
}