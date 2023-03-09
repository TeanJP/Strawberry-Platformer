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

    private bool bouncing = false;
    #endregion

    #region Components
    private Rigidbody2D rb = null;
    private SpriteRenderer spriteRenderer = null;
    private BoxCollider2D activeCollider = null;
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
    private float jumpBuffer = 0.2f;
    private float jumpTimer = 0f;

    private float groundedBuffer = 0.15f;
    private float groundedTimer = 0f;
    #endregion

    #region Movement Values
    private float currentSpeed = 0f;
    private float previousSpeed;

    private float initialWalkSpeed = 2f;
    private float maxWalkSpeed = 5f;
    private float walkAcceleration = 6f;

    private float initialRunSpeed = 5f;
    private float maxRunSpeed = 10f;
    private float runAcceleration = 5f;
    
    private float speedReduction = 1f;

    private float jumpStrength = 6f;
    private float fallSpeed = 2.5f;
    private float incompleteJumpStrength = 0.25f;

    private float crawlSpeed = 3f;
    private float crawlJumpStrength = 4f;

    private float bellyFlopStrength = 8f;

    private float turnDeceleration = 6f;

    private float slideDeceleration = 6f;

    private float diveSpeed = 10f;
    private Vector2 diveDirection = new Vector2(1f, -1f);

    private float wallRunSpeed = 6f;

    private float wallJumpStrength = 6f;
    private Vector2 wallJumpDirection = new Vector2(1f ,1f);

    private float superJumpSpeed = 10f;
    private float superJumpChargeSpeed = 3f;

    private float superJumpCancelSpeed = 10f;
    private Vector2 superJumpCancelDirection = new Vector2(1f, 0f);

    private float knockBackStrength = 1f;
    private Vector2 knockBackDirection = new Vector2(-1f, 1f);
    #endregion

    #region Health
    private int maxHearts = 100;
    private int hearts = 0;

    private float stunDuration = 0.5f;
    private float stunTimer = 0f;

    private float invincibilityDuratrion = 3f;
    private float invincibilityTimer = 0f;

    private GameObject heart = null;
    #endregion

    [SerializeField]
    private LayerMask platformMask;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = fullSprite;
        activeCollider = fullCollider;

        diveDirection.Normalize();
        wallJumpDirection.Normalize();
        superJumpCancelDirection.Normalize();
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
        DecrementTimers(Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D other)
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
            if (runState == RunState.Default && grounded && isWall)
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
            if (downInput)
            {
                movementState = MovementState.Crawling;
            }
            else
            {
                movementState = MovementState.Default;
                SwapActiveCollider();
            }

            rb.velocity = Vector2.zero;
        }

        if (applyStun)
        {
            ApplyStun();
            ApplyKnockBack(GetPlayerDirection() * -1f);
        }
    }

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

                if (grounded && (runInput || currentSpeed > maxWalkSpeed))
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
                        if (!runInput)
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
                                }

                                SwapActiveCollider();
                            }
                            else if (!grounded && hittingWall)
                            {
                                runState = RunState.WallRunning;
                                currentSpeed = wallRunSpeed;
                            }
                            else if (upInput && grounded)
                            {
                                runState = RunState.ChargingSuperJump;
                                SwapActiveCollider();
                            }
                            else if (GetFacingIncorrectDirection() && grounded)
                            {
                                FlipPlayerDirection();
                                runState = RunState.Turning;
                                previousSpeed = currentSpeed;
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
                            else
                            {
                                runState = RunState.Stopping;
                            }

                            SwapActiveCollider();
                        }
                        else if (downInput && !grounded)
                        {
                            runState = RunState.Diving;
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
                        }
                        else if (grounded)
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
                        break;
                }
                break;
            case MovementState.Crawling:
                if (!downInput && !hittingCeiling && grounded)
                {
                    movementState = MovementState.Default;
                    SwapActiveCollider();
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

                if (jumpTimer > 0f && groundedTimer > 0f)
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

                        if (jumpTimer > 0f && groundedTimer > 0f)
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

                        if (currentSpeed == 0f)
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
                movement.x = horizontalDirection * crawlSpeed;

                if (jumpTimer > 0f && groundedTimer > 0f)
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
                    movementApplied = true;
                }
                break;
        }

        if (movementState == MovementState.Running && (runState == RunState.Diving || runState == RunState.WallRunning || runState == RunState.SuperJumping))
        {
            rb.gravityScale = 0f;
        }
        else
        {
            if (movement.y < 0f)
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

    #region Player Direction
    private bool GetFacingIncorrectDirection()
    {
        return horizontalInput != 0f && horizontalInput != Mathf.Sign(transform.localScale.x);
    }

    private void FlipPlayerDirection()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

    private float GetPlayerDirection()
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
    }

    #region Collision Checks
    private bool GetGrounded()
    { 
        float boxHeight = 0.01f;
        Vector2 boxCheckPosition =  new Vector2(transform.position.x, activeCollider.bounds.min.y - boxHeight * 0.5f);
        Vector2 boxCheckSize = new Vector2(activeCollider.bounds.extents.x * 2f, boxHeight);

        Collider2D[] platforms = Physics2D.OverlapBoxAll(boxCheckPosition, boxCheckSize, 0f, platformMask);

        if (platforms.Length != 0)
        {
            for (int i = 0; i < platforms.Length; i++)
            {
                if (GetAboveCollider(platforms[i]))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private bool GetHittingWall()
    {   
        float boxWidth = 0.01f;
        Vector2 boxCheckPosition = new Vector2(activeCollider.bounds.center.x + (activeCollider.bounds.extents.x + (boxWidth * 0.5f)) * GetPlayerDirection(), activeCollider.bounds.center.y);
        Vector2 boxCheckSize = new Vector2(boxWidth, activeCollider.bounds.extents.y * 2f);

        Collider2D[] platforms = Physics2D.OverlapBoxAll(boxCheckPosition, boxCheckSize, 0f, platformMask);

        if (platforms.Length != 0)
        {
            for (int i = 0; i < platforms.Length; i++)
            {
                if (GetParallelToCollider(platforms[i]))
                {
                    return true;
                }
            }
        }

        return false;
    }
        
    private bool GetHittingCeiling()
    {
        float boxHeight = 0.01f;
        Vector2 boxCheckPosition = new Vector2(transform.position.x, activeCollider.bounds.max.y + boxHeight * 0.5f);
        Vector2 boxCheckSize = new Vector2(activeCollider.bounds.extents.x * 2f, boxHeight);

        Collider2D[] platforms = Physics2D.OverlapBoxAll(boxCheckPosition, boxCheckSize, 0f, platformMask);

        if (platforms.Length != 0)
        {
            for (int i = 0; i < platforms.Length; i++)
            {
                if (GetBelowCollider(platforms[i]))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Collider Position Comparisons
    private bool GetAboveCollider(Collider2D other)
    {
        return activeCollider.bounds.min.y > other.bounds.max.y;
    }

    private bool GetParallelToCollider(Collider2D other)
    {
        return activeCollider.bounds.min.y < other.bounds.max.y && activeCollider.bounds.max.y > other.bounds.min.y;
    }

    private bool GetBelowCollider(Collider2D other)
    {
        return activeCollider.bounds.max.y < other.bounds.min.y;
    }
    #endregion

    public void TakeDamge(int damage, float horizontalDirection)
    {
        if (invincibilityTimer <= 0f)
        {
            if (hearts == 0)
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
                ApplyKnockBack(horizontalDirection);
            }
        }
    }

    private void SpawnHearts(int amount)
    {


        for (int i = 0; i < amount; i++)
        {
            Instantiate(heart);
        }
    }

    private void ApplyStun()
    {
        movementState = MovementState.Stunned;
        stunTimer = stunDuration;
    }

    private void ApplyKnockBack(float horizontalDirection)
    {
        rb.velocity = knockBackDirection * horizontalDirection * knockBackStrength;
    }

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
    }
}