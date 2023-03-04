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

    private float currentSpeed = 0f;
    private float previousSpeed;

    private float initialWalkSpeed;
    private float maxWalkSpeed;
    private float walkAcceleration;

    private float initialRunSpeed;
    private float maxRunSpeed;
    private float runAcceleration;
    private float runDamping;

    private float jumpStrength;
    private float fallSpeed;
    private float incompleteJumpStrength;

    private float crawlSpeed;
    private float crawlJumpStrength;

    private float bellyFlopStrength;

    private float turnDeceleration;

    private float slideDeceleration;

    private float diveSpeed;
    private Vector2 diveDirection;

    private float wallRunSpeed;

    private float wallJumpStrength;
    private Vector2 wallJumpDirection;

    private float superJumpSpeed;
    private float superJumpChargeSpeed;

    private float superJumpCancelSpeed;
    private Vector2 superJumpCancelDirection;


    void Start()
    {
        
    }

    void Update()
    {
        bool grounded = GetGrounded();
        bool hittingWall = GetHittingWall();
        bool hittingCeiling = GetHittingCeiling();

        GetInputs();
        ApplyInputs(grounded, hittingWall, hittingCeiling);
        Move(grounded);
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
                        break;
                    case RunState.Rolling:

                        break;
                    case RunState.Turning:

                        break;
                    case RunState.Stopping:

                        break;
                    case RunState.WallRunning:
                        if (jumpTimer > 0f)
                        {
                            runState = RunState.WallJumping;
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
                            runState = RunState.CancellingSuperJump;
                        }
                        break;
                    case RunState.ChargingSuperJump:
                        if (!upInput)
                        {
                            runState = RunState.SuperJumping;
                        }
                        break;
                    case RunState.CancellingSuperJump:

                        break;
                }
                break;
            case MovementState.Crawling:
                if (!downInput && !hittingCeiling)
                {
                    movementState = MovementState.Default;
                }
                break;
            case MovementState.BellyFlopping:

                break;
        }
    }

    private void Move(bool grounded)
    {
        switch (movementState)
        {
            case MovementState.Default:

                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:

                        break;
                    case RunState.Rolling:

                        break;
                    case RunState.Turning:

                        break;
                    case RunState.Stopping:

                        break;
                    case RunState.WallRunning:

                        break;
                    case RunState.WallJumping:

                        break;
                    case RunState.Diving:

                        break;
                    case RunState.SuperJumping:

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
    }

    private void Run()
    {

    }

    private bool GetGrounded()
    {
        /*
        float boxHeight = 0.01f;
        Vector2 boxCheckPosition =  new Vector2(transform.position.x, transform.position.y - spriteDimensions.y - boxHeight * 0.5f);
        Vector2 boxCheckSize = new Vector2(spriteDimensions.x * 2f, boxHeight);

        Collider2D[] platforms = Physics2D.OverlapBoxAll(boxCheckPosition, boxCheckSize, 0f, platformMask);

        if (platforms.Length != 0)
        {
            for (int i = 0; i < platforms.Length; i++)
            {
                if ((platforms[i].transform.position.y + platforms[i].bounds.extents.y) < transform.position.y - spriteDimensions.y)
                {
                    return true;
                }
            }
        }
        */
        return false;
    }

    private bool GetHittingWall()
    {
        /*
        float boxWidth = 0.01f;
        Vector2 boxCheckPosition = new Vector2(transform.position.x + spriteDimensions.x * Mathf.Sign(transform.localScale.x) + boxWidth * 0.5f, transform.position.y);
        Vector2 boxCheckSize = new Vector2(boxWidth, spriteDimensions.y);

        Collider2D platform = Physics2D.OverlapBox(boxCheckPosition, boxCheckSize, 0f, platformMask);

        return platform != null;
        */
        return false;
    }
        
    private bool GetHittingCeiling()
    {
        return false;
    }
}
