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

    private bool movementApplied = false;

    private bool bouncing = false;
    #endregion

    #region Components
    private Rigidbody2D rb = null;
    private SpriteRenderer spriteRenderer = null;
    private BoxCollider2D activeCollider = null;
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
    private float walkAcceleration = 1f;

    private float initialRunSpeed = 5f;
    private float maxRunSpeed = 10f;
    private float runAcceleration = 2f;
    private float runDamping = 1f;

    private float jumpStrength = 6f;
    private float fallSpeed = 2.5f;
    private float incompleteJumpStrength = 0.5f;

    private float crawlSpeed = 3f;
    private float crawlJumpStrength = 4f;

    private float bellyFlopStrength = 8f;

    private float turnDeceleration = 1f;

    private float slideDeceleration = 1f;

    private float diveSpeed = 10f;
    private Vector2 diveDirection = new Vector2(1f, -1f);

    private float wallRunSpeed = 6f;

    private float wallJumpStrength = 6f;
    private Vector2 wallJumpDirection = new Vector2(1f ,1f);

    private float superJumpSpeed = 10f;
    private float superJumpChargeSpeed = 3f;

    private float superJumpCancelSpeed = 10f;
    private Vector2 superJumpCancelDirection = new Vector2(1f, 0f);
    #endregion

    #region Health
    private int maxHearts = 100;
    private int hearts = 0;
    #endregion

    [SerializeField]
    private LayerMask platformMask;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        activeCollider = GetComponent<BoxCollider2D>();

        diveDirection.Normalize();
        wallJumpDirection.Normalize();
        superJumpCancelDirection.Normalize();
    }

    void Update()
    {
        bool grounded = GetGrounded();
        
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
        //Check if the player crashes into the ceiling or a wall whilst running, belly flopping, super jumping or diving.
        //If so stun the player and bounce them away, putting them back into the default state.
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

                if (grounded && runInput)
                {
                    movementState = MovementState.Running;
                    runState = RunState.Default;
                }

                if (downInput)
                {
                    if (grounded)
                    {
                        movementState = MovementState.Crawling;
                    }
                    else
                    {
                        movementState = MovementState.BellyFlopping;
                    }
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (GetFacingIncorrectDirection())
                        {
                            FlipPlayerDirection();
                            runState = RunState.Turning;
                        }
                        
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
                        }

                        if (!grounded && hittingWall)
                        {
                            runState = RunState.WallRunning;
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
                        }

                        if (downInput && !grounded)
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
                                    runState = RunState.WallJumping;
                                }
                            }
                            else
                            {
                                runState = RunState.Default;
                            }
                        }
                        else
                        {
                            movementState = MovementState.Default;
                            runState = RunState.Default;
                        }                      
                        break;
                    case RunState.WallJumping:
                        if (downInput)
                        {
                            runState = RunState.Diving;
                        }
                        break;
                    case RunState.Diving:
                        if (downInput)
                        {
                            movementState = MovementState.BellyFlopping;
                            runState = RunState.Default;
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
                        }
                        break;
                    case RunState.ChargingSuperJump:
                        if (!upInput)
                        {
                            runState = RunState.SuperJumping;
                        }
                        break;
                }
                break;
            case MovementState.Crawling:
                if (!downInput && !hittingCeiling)
                {
                    movementState = MovementState.Default;
                }
                break;
        }
    }

    private void Move(bool grounded)
    {
        Vector2 movement = rb.velocity;

        switch (movementState)
        {
            case MovementState.Default:
                if (horizontalInput == 0f)
                {
                    currentSpeed = initialWalkSpeed;
                }
                else
                {
                    currentSpeed = Mathf.Min(currentSpeed + walkAcceleration * Time.deltaTime, maxWalkSpeed);
                }

                movement.x = horizontalInput * currentSpeed;          

                if (jumpTimer > 0f && groundedTimer > 0f)
                {
                    movement.y = jumpStrength;
                }

                if (rb.velocity.y < 0f)
                {
                    rb.gravityScale = fallSpeed;
                }
                else
                {
                    rb.gravityScale = 1f;
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (grounded)
                        {
                            currentSpeed = Mathf.Min(currentSpeed + runAcceleration * Time.deltaTime, maxRunSpeed);
                        }

                        if (jumpTimer > 0f && groundedTimer > 0f)
                        {
                            movement.y = jumpStrength;
                        }

                        break;
                    case RunState.Rolling:

                        break;
                    case RunState.Turning:
                        //Change state when turn complete.
                        break;
                    case RunState.Stopping:
                        //Change state when speed fully reduced.
                        break;
                    case RunState.WallRunning:

                        break;
                    case RunState.WallJumping:

                        break;
                    case RunState.Diving:

                        break;
                    case RunState.SuperJumping:
                        movement = new Vector2(0f, superJumpSpeed);
                        break;
                    case RunState.ChargingSuperJump:

                        break;
                    case RunState.CancellingSuperJump:

                        break;
                }
                break;
            case MovementState.Crawling:

                break;
            case MovementState.BellyFlopping:

                break;
            case MovementState.Stunned:

                break;
        }

        rb.velocity = movement;
    }

    private bool GetFacingIncorrectDirection()
    {
        return horizontalInput != 0f && horizontalInput != Mathf.Sign(transform.localScale.x);
    }

    private void FlipPlayerDirection()
    {
        transform.localPosition = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

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
        Vector2 boxCheckPosition = new Vector2(activeCollider.bounds.max.x * Mathf.Sign(transform.localScale.x) + boxWidth * 0.5f, transform.position.y);
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
}