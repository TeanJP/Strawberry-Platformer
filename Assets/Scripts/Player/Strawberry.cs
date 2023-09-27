using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Strawberry : MonoBehaviour
{
    #region Enums
    private enum MovementState
    {
        Default,
        Running,
        Crawling,
        BellyFlopping,
        Stunned,
        Defeated
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

    private enum StunType
    {
        Collision,
        Damage
    }

    private enum AttackDirection
    {
        Horizontal,
        Vertical
    }

    private enum VerticalDirection
    {
        Above,
        Below
    }
    #endregion

    #region States
    private MovementState movementState = MovementState.Default;
    private RunState runState = RunState.Default;

    private bool grounded = false;

    private bool movementApplied = false;

    private bool bouncing = false;

    private string currentAnimation = "Strawberry Idle";
    #endregion

    #region Collision Checking
    [Header("Collision Checking")]
    [SerializeField]
    private LayerMask platformMask;
    [SerializeField]
    private LayerMask nonBreakableWallMask;
    [SerializeField]
    private LayerMask enemyMask;
    [SerializeField]
    private LayerMask breakableBlockMask;
    [SerializeField]
    private float attackCheckWidth = 0.04f;
    [SerializeField]
    private float attackReduction = 0.02f;
    private float raycastLeniency = 0.02f;
    private float raycastLength = 0.02f;
    private int horizontalRaycasts = 3;
    private float horizontalRaycastSpacing;
    private int verticalRaycasts = 3;
    private float verticalRaycastSpacing;
    private float colliderHeightDifference;
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
    private CapsuleCollider2D fullTrigger = null;
    [SerializeField]
    private CapsuleCollider2D halfTrigger = null;
    private Animator animator = null;
    #endregion

    #region Managers
    private GameManager gameManager = null;
    private ScoreManager scoreManager = null;
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
    private float bounceStrength = 5f;

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
    private int hearts = 0;

    [SerializeField]
    private float damageStunDuration = 1f;
    [SerializeField]
    private float collisionStunDuration = 0.5f;
    private float stunTimer = 0f;

    [SerializeField]
    private float invincibilityDuratrion = 3f;
    private float invincibilityTimer = 0f;

    [SerializeField]
    private GameObject droppedHeart = null;
    [SerializeField]
    private int maxDroppedHearts = 10;
    [SerializeField]
    private float dropSpeed = 4f;

    [SerializeField]
    private float heartActivationDistance = 1.5f;
    [SerializeField]
    private LayerMask heartMask;
    #endregion

    #region Attack Values
    [Header("Attack Values")]
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

    #region HUD Elements
    private TextMeshProUGUI heartsCounter = null;
    private Image heartsLevel = null;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        gameManager = GameManager.Instance;
        scoreManager = gameManager.GetScoreManager();

        colliderHeightDifference = fullCollider.bounds.size.y - halfCollider.bounds.size.y;
        
        halfCollider.enabled = false;
        
        activeCollider = fullCollider;

        Vector2 activeColliderDimensions = activeCollider.bounds.size;

        verticalRaycastSpacing = activeColliderDimensions.x / (verticalRaycasts - 1);
        horizontalRaycastSpacing = activeColliderDimensions.y / (horizontalRaycasts - 1);

        diveDirection.Normalize();
        wallJumpDirection.Normalize();
        superJumpCancelDirection.Normalize();
        collisionRepelDirection.Normalize();

        Transform heartsDisplay = gameManager.GetStrawberryHeartsDisplay().transform;
        heartsCounter = heartsDisplay.GetChild(1).GetComponent<TextMeshProUGUI>();
        heartsLevel = heartsDisplay.GetChild(2).GetChild(1).GetComponent<Image>();

        UpdateHeartsDisplay();
    }

    void Update()
    {
        GetInputs();

        if (!gameManager.GetGamePaused())
        {
            grounded = GetVerticalCollision(platformMask, VerticalDirection.Below);

            if (grounded)
            {
                groundedTimer = groundedBuffer;
                bouncing = false;
            }

            bool hittingWall = GetHittingWall(platformMask);
            bool hittingNonBreakableWall = GetHittingWall(nonBreakableWallMask);
            bool hittingCeiling = GetVerticalCollision(platformMask, VerticalDirection.Above);
            bool hittingNonBreakableCeiling = GetVerticalCollision(nonBreakableWallMask, VerticalDirection.Above);
            bool hittingNonBreakableFloor = GetVerticalCollision(nonBreakableWallMask, VerticalDirection.Below);

            ApplyInputs(grounded, hittingWall, hittingNonBreakableWall, hittingCeiling, hittingNonBreakableCeiling, hittingNonBreakableFloor);
            Move(grounded);
            Attack();
            DecrementTimers(Time.deltaTime);
            ApplyAnimation();

            if (movementState != MovementState.Stunned)
            {
                ActivateHearts();
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

    private void ApplyInputs(bool grounded, bool hittingWall, bool hittingNonBreakableWall, bool hittingCeiling, bool hittingNonBreakableCeiling, bool hittingNonBreakableFloor)
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
                        if (hittingNonBreakableWall && grounded)
                        {
                            ApplyStun(StunType.Collision);
                            RepelPlayer(collisionRepelDirection * new Vector2(GetPlayerDirection() * -1f, 1f), collisionRepelStrength);
                        }
                        else if (!runInput && boostTimer <= 0f && grounded)
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
                            else if (!grounded && hittingNonBreakableWall)
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
                        if (hittingNonBreakableWall)
                        {
                            ApplyStun(StunType.Collision);
                            RepelPlayer(collisionRepelDirection * new Vector2(GetPlayerDirection() * -1f, 1f), collisionRepelStrength);
                        }
                        else if (!downInput && !hittingCeiling)
                        {
                            if (runInput)
                            {
                                runState = RunState.Default;
                                SwapActiveCollider();
                            }
                            else if (boostTimer <= 0f)
                            {
                                runState = RunState.Stopping;
                                SwapActiveCollider();
                            }
                        }
                        else if (downInput && !grounded)
                        {
                            runState = RunState.Diving;
                            boostTimer = 0f;
                        }
                        break;
                    case RunState.Stopping:
                        if (hittingNonBreakableWall)
                        {
                            ApplyStun(StunType.Collision);
                            RepelPlayer(collisionRepelDirection * new Vector2(GetPlayerDirection() * -1f, 1f), collisionRepelStrength);
                        }
                        break;
                    case RunState.WallRunning:
                        if (hittingNonBreakableCeiling)
                        {
                            movementState = MovementState.Default;
                            runState = RunState.Default;
                            rb.velocity = Vector2.zero;
                        }
                        else if (runInput)
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
                        else if (hittingNonBreakableWall)
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
                        if (hittingNonBreakableWall)
                        {
                            ApplyStun(StunType.Collision);
                            RepelPlayer(collisionRepelDirection * new Vector2(GetPlayerDirection() * -1f, 1f), collisionRepelStrength);
                        }
                        else if (jumpTimer > 0f)
                        {
                            movementState = MovementState.BellyFlopping;
                            runState = RunState.Default;
                            
                            movementApplied = false;
                            jumpTimer = 0f;
                        }
                        else if (grounded && !hittingNonBreakableWall)
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
                        if (hittingNonBreakableCeiling)
                        {
                            movementState = MovementState.Default;
                            runState = RunState.Default;
                            rb.velocity = Vector2.zero;
                        }
                        else if (jumpTimer > 0f)
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
                        if (GetFacingIncorrectDirection())
                        {
                            FlipPlayerDirection();
                        }

                        if (!upInput && grounded && !hittingCeiling)
                        {
                            runState = RunState.SuperJumping;
                            SwapActiveCollider();
                        }
                        break;
                    case RunState.CancellingSuperJump:
                        if (hittingNonBreakableWall)
                        {
                            if (runInput)
                            {
                                runState = RunState.WallRunning;
                                currentSpeed = wallRunSpeed;
                            }
                            else
                            {
                                movementState = MovementState.Default;
                                runState = RunState.Default;
                                rb.velocity = Vector2.zero;
                            }
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
                if (hittingNonBreakableFloor && !movementApplied)
                {
                    flopRecoveryTimer = flopRecoveryDuration;
                    movementApplied = true;
                }
                else if (!grounded && flopRecoveryTimer > 0f)
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

                if (movement.y > 0f && releasedJumpInput && !movementApplied && !bouncing)
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

                        if (movement.y > 0f && releasedJumpInput && !movementApplied && !bouncing)
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

                if (movement.y > 0f && releasedJumpInput && !movementApplied && !bouncing)
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

    private void ApplyAnimation()
    {
        string animationToPlay = currentAnimation;

        switch (movementState)
        {
            case MovementState.Default:
                if (rb.velocity.y > 0f)
                {
                    animationToPlay = "Strawberry Jump";
                }
                else
                {
                    if (grounded)
                    {
                        if (horizontalInput == 0f)
                        {
                            animationToPlay = "Strawberry Idle";
                        }
                        else
                        {
                            animationToPlay = "Strawberry Walk";
                        }
                    }
                    else
                    {
                        animationToPlay = "Strawberry Fall";
                    }
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (grounded)
                        {
                            animationToPlay = "Strawberry Run";
                        }
                        else
                        {
                            animationToPlay = "Strawberry Run Jump";
                        }
                        break;
                    case RunState.Rolling:
                        animationToPlay = "Strawberry Roll";
                        break;
                    case RunState.Turning:
                        animationToPlay = "Strawberry Run Turn";
                        break;
                    case RunState.Stopping:
                        animationToPlay = "Strawberry Run Drift";
                        break;
                    case RunState.WallRunning:
                        animationToPlay = "Strawberry Wall Run";
                        break;
                    case RunState.WallJumping:
                        animationToPlay = "Strawberry Run Jump";
                        break;
                    case RunState.Diving:
                        animationToPlay = "Strawberry Dive";
                        break;
                    case RunState.SuperJumping:
                        animationToPlay = "Strawberry Super Jump";
                        break;
                    case RunState.ChargingSuperJump:
                        if (horizontalInput == 0f || !grounded)
                        {
                            animationToPlay = "Strawberry Charge Idle";
                        }
                        else
                        {
                            animationToPlay = "Strawberry Charge Move";
                        }
                        break;
                    case RunState.CancellingSuperJump:
                        animationToPlay = "Strawberry Run Jump";
                        break;
                }
                break;
            case MovementState.Crawling:
                if (rb.velocity.y > 0f)
                {
                    animationToPlay = "Strawberry Dive";
                }
                else
                {
                    if (grounded)
                    {
                        if (horizontalInput == 0f)
                        {
                            animationToPlay = "Strawberry Crawl Idle";
                        }
                        else
                        {
                            animationToPlay = "Strawberry Crawl";
                        }
                    }
                    else
                    {
                        animationToPlay = "Strawberry Lying Fall";
                    }
                }
                break;
            case MovementState.BellyFlopping:
                animationToPlay = "Strawberry Flop";
                break;
            case MovementState.Stunned:
                if (activeCollider == halfCollider)
                {
                    animationToPlay = "Strawberry Lying Fall";
                }
                else
                {
                    animationToPlay = "Strawberry Fall";
                }
                break;
            case MovementState.Defeated:
                if (activeCollider == halfCollider)
                {
                    animationToPlay = "Strawberry Lying Fall";
                }
                else
                {
                    animationToPlay = "Strawberry Fall";
                }
                break;
        }

        if (animationToPlay != currentAnimation)
        {
            animator.Play(animationToPlay);
            currentAnimation = animationToPlay;
        }
    }

    #region Player Direction
    private bool GetFacingIncorrectDirection()
    {
        float currentDirection = GetPlayerDirection();
        return horizontalInput != 0f && horizontalInput != currentDirection;
    }

    private void FlipPlayerDirection()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

    public float GetPlayerDirection()
    {
        return Mathf.Sign(transform.localScale.x) * -1f;
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
                if (activeCollider == fullCollider)
                {
                    movementState = MovementState.Default;
                }
                else
                {
                    movementState = MovementState.Crawling;
                }
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
    private bool GetHittingWall(LayerMask maskToUse)
    {
        Vector2 raycastDirection = new Vector2(GetPlayerDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + activeCollider.bounds.extents.x * GetPlayerDirection(), activeCollider.bounds.min.y);

        for (int i = 0; i < horizontalRaycasts; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, maskToUse);

            if (hit.collider != null)
            {
                return true;
            }

            raycastOrigin.y += horizontalRaycastSpacing;
        }

        return false;
    }    

    private bool GetVerticalCollision(LayerMask maskToUse, VerticalDirection verticalDirection)
    {
        Vector2 raycastDirection, raycastOrigin;

        float raycastLength = this.raycastLength;

        if (verticalDirection == VerticalDirection.Above)
        {
            raycastDirection = Vector2.up;
            raycastOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.max.y);

            if (activeCollider == halfCollider)
            {
                raycastLength = colliderHeightDifference;
            }
        }
        else
        {
            raycastDirection = Vector2.down;
            raycastOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.min.y);
        }

        for (int i = 0; i < verticalRaycasts; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, maskToUse);

            if (hit.collider != null)
            {
                return true;
            }

            raycastOrigin.x += verticalRaycastSpacing;
        }

        raycastOrigin.x = activeCollider.bounds.min.x - raycastLeniency;

        Vector2 wallCheckDirection = Vector2.left;
        Vector2 wallCheckOrigin;

        if (verticalDirection == VerticalDirection.Above)
        {
            wallCheckOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.max.y);
        }
        else
        {
            wallCheckOrigin = new Vector2(activeCollider.bounds.min.x, activeCollider.bounds.min.y);
        }

        for (int i = 0; i < 2; i++)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheckOrigin, wallCheckDirection, this.raycastLength, maskToUse);

            if (wallHit.collider == null)
            {
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, maskToUse);

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
    #endregion

    #region Attacks
    private void Attack()
    {
        bool falling = rb.velocity.y <= 0f;

        switch (movementState)
        {
            case MovementState.Default:
                if (!grounded && falling)
                {
                    PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                }
                break;
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Default:
                        if (currentSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(fullDamage, fullRepelStrength, true);
                        }
                        else if (currentSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(reducedDamage, reducedRepelStrength, true);
                        }
                        else
                        {
                            PerformHorizontalAttack(minimumDamage, minimumRepelStrength, true);
                        }

                        if (!grounded && falling)
                        {
                            PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                        }
                        break;
                    case RunState.Rolling:
                        if (currentSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(fullDamage, fullRepelStrength, true);
                        }
                        else if (currentSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(reducedDamage, reducedRepelStrength, true);
                        }
                        else
                        {
                            PerformHorizontalAttack(minimumDamage, minimumRepelStrength, true);
                        }
                        break;
                    case RunState.Turning:
                        if (previousSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(fullDamage, fullRepelStrength, true);
                        }
                        else if (previousSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(reducedDamage, reducedRepelStrength, true);
                        }
                        else
                        {
                            PerformHorizontalAttack(minimumDamage, minimumRepelStrength, true);
                        }

                        if (!grounded && falling)
                        {
                            PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                        }
                        break;
                    case RunState.Stopping:
                        if (currentSpeed >= maxRunSpeed)
                        {
                            PerformHorizontalAttack(fullDamage, fullRepelStrength, true);
                        }
                        else if (currentSpeed >= minimumSpeed)
                        {
                            PerformHorizontalAttack(reducedDamage, reducedRepelStrength, true);
                        }
                        else
                        {
                            PerformHorizontalAttack(minimumDamage, minimumRepelStrength, true);
                        }
                        
                        if (!grounded && falling)
                        {
                            PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                        }
                        break;
                    case RunState.WallRunning:
                        PerformUpwardsAttack(reducedDamage, reducedRepelStrength, true);
                        break;
                    case RunState.WallJumping:
                        PerformHorizontalAttack(reducedDamage, reducedRepelStrength, true);
                        PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                        break;
                    case RunState.Diving:
                        PerformHorizontalAttack(reducedDamage, reducedRepelStrength, true);
                        PerformDownwardsAttack(reducedDamage, reducedRepelStrength, true, false);
                        break;
                    case RunState.SuperJumping:
                        PerformUpwardsAttack(fullDamage, fullRepelStrength, true);
                        break;
                    case RunState.CancellingSuperJump:
                        PerformHorizontalAttack(fullDamage, fullRepelStrength, true);
                        PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                        break;
                }
                break;
            case MovementState.Crawling:
                if (!grounded && falling)
                {
                    PerformDownwardsAttack(reducedDamage, reducedRepelStrength, false, true);
                }
                break;
            case MovementState.BellyFlopping:
                if (flopRecoveryTimer <= 0f)
                {
                    PerformDownwardsAttack(fullDamage, fullRepelStrength, true, false);
                }
                break;
            case MovementState.Stunned:
                if (!grounded && falling)
                {
                    PerformDownwardsAttack(minimumDamage, minimumRepelStrength, false, true);
                }
                break;
        }
    }


    private void PerformHorizontalAttack(int damage, float repelStrength, bool breakBlocks)
    {
        float horizontalDirection = GetPlayerDirection();

        if (movementState == MovementState.Running && runState == RunState.Turning)
        {
            horizontalDirection *= -1f;
        }

        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x + (activeCollider.bounds.extents.x + attackCheckWidth * 0.5f) * horizontalDirection, activeCollider.bounds.center.y);
        Vector2 boxSize = new Vector2(attackCheckWidth, activeCollider.bounds.size.y - attackReduction * 2f);

        DealDamage(damage, repelStrength, breakBlocks, false, boxPosition, boxSize, AttackDirection.Horizontal);
    }
    
    private void PerformDownwardsAttack(int damage, float repelStrength, bool breakBlocks, bool bounce)
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x, activeCollider.bounds.min.y - attackCheckWidth * 0.5f);
        Vector2 boxSize = new Vector2(activeCollider.bounds.size.x - attackReduction * 2f, attackCheckWidth);

        bool attackSuccessful = DealDamage(damage, repelStrength, breakBlocks, true, boxPosition, boxSize, AttackDirection.Vertical);

        if (attackSuccessful && bounce)
        {
            Bounce();
        }
    }

    private void PerformUpwardsAttack(int damage, float repelStrength, bool breakBlocks)
    {
        Vector2 boxPosition = new Vector2(activeCollider.bounds.center.x, activeCollider.bounds.max.y + attackCheckWidth * 0.5f);
        Vector2 boxSize = new Vector2(activeCollider.bounds.size.x - attackReduction * 2f, attackCheckWidth);

        DealDamage(damage, repelStrength, breakBlocks, false, boxPosition, boxSize, AttackDirection.Vertical);
    }

    private bool DealDamage(int damage, float repelStrength, bool breakBlocks, bool destroyProjectiles, Vector2 boxPosition, Vector2 boxSize, AttackDirection attackDirection)
    {
        Collider2D[] enemies = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, enemyMask);

        bool attackSuccessful = false;

        for (int i = 0; i < enemies.Length; i++)
        {
            bool enemy = enemies[i].CompareTag("Enemy");
            bool launchedEnemy = enemies[i].CompareTag("Launched Enemy");

            if (enemy)
            {
                Enemy currentEnemy = enemies[i].GetComponent<Enemy>();

                if (!attackSuccessful)
                {
                    attackSuccessful = true;
                }

                Vector2 repelDirection = Vector2.zero;

                switch (attackDirection)
                {
                    case AttackDirection.Horizontal:
                        float horizontalDirection = GetPlayerDirection();
                        
                        if (movementState == MovementState.Running && runState == RunState.Turning)
                        {
                            horizontalDirection *= -1f;
                        }

                        repelDirection = new Vector2(horizontalDirection, 1f);
                        break;
                    case AttackDirection.Vertical:
                        repelDirection = new Vector2(Mathf.Sign(currentEnemy.transform.position.x - boxPosition.x), 1f);
                        break;
                }                
                
                repelDirection.Normalize();
                currentEnemy.TakeDamage(false, damage, attackStunDuration, repelDirection, repelStrength);
            }
            else if (launchedEnemy && destroyProjectiles)
            {
                LaunchedEnemy currentLaunchedEnemy = enemies[i].GetComponent<LaunchedEnemy>();

                if (currentLaunchedEnemy.GetDirection() != Vector2.up)
                {
                    if (!attackSuccessful)
                    {
                        attackSuccessful = true;
                    }

                    currentLaunchedEnemy.Crash();
                }
            }
        }

        if (breakBlocks)
        {
            Collider2D[] breakableBlocks = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, breakableBlockMask);

            for (int i = 0; i < breakableBlocks.Length; i++)
            {
                BreakableBlock breakableBlock = breakableBlocks[i].GetComponent<BreakableBlock>();

                if (breakableBlock != null)
                {
                    breakableBlock.Break();
                }
            }
        }

        return attackSuccessful;
    }
    #endregion

    #region Taking Damage
    public void TakeDamge(int damage, Vector2 repelDirection, float repelStrength)
    {
        if (invincibilityTimer <= 0f)
        {
            if (hearts == 0 || damage > maxHearts)
            {
                SetDefeated();
            }
            else
            {
                int previousHearts = hearts;
                hearts = Mathf.Max(hearts - damage, 0);

                Vector2 dropDirection = new Vector2(Mathf.Sign(repelDirection.x), 0f);
                SpawnHearts(Mathf.Min(previousHearts - hearts, maxDroppedHearts), dropDirection);

                invincibilityTimer = invincibilityDuratrion;
                ApplyStun(StunType.Damage);
            }

            RepelPlayer(repelDirection, repelStrength);

            UpdateHeartsDisplay();
        }
    }

    private void ApplyStun(StunType stunType)
    {
        movementState = MovementState.Stunned;
        runState = RunState.Default;
        movementApplied = false;
        currentSpeed = 0f;

        switch (stunType)
        {
            case StunType.Collision:
                stunTimer = collisionStunDuration;
                break;
            case StunType.Damage:
                stunTimer = damageStunDuration;
                break;
        }
    }

    public void RepelPlayer(Vector2 repelDirection, float repelStrength)
    {
        rb.velocity = repelDirection * repelStrength;
    }

    public void SetDefeated()
    {
        if (movementState != MovementState.Defeated)
        {
            movementState = MovementState.Defeated;

            activeCollider.enabled = false;

            if (fullTrigger.enabled)
            {
                fullTrigger.enabled = false;
            }
            else
            {
                halfTrigger.enabled = false;
            }

            GameObject leche = transform.GetChild(1).gameObject;
            leche.GetComponent<Leche>().enabled = false;
            leche.transform.parent = null;

            GameManager.Instance.SetGameOver();
        }
    }
    #endregion

    private void SwapActiveCollider()
    {
        fullCollider.enabled = !fullCollider.enabled;
        halfCollider.enabled = !halfCollider.enabled;

        fullTrigger.enabled = !fullTrigger.enabled;
        halfTrigger.enabled = !halfTrigger.enabled;

        if (halfCollider.enabled)
        {
            activeCollider = halfCollider;
        }
        else
        {
            activeCollider = fullCollider;
        }
        
        horizontalRaycastSpacing = activeCollider.bounds.size.y / (horizontalRaycasts - 1);
    }

    #region Enemy Interaction
    public bool GetRunning()
    {
        return movementState == MovementState.Running;
    }

    public bool GetDefeated()
    {
        return movementState == MovementState.Defeated;
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

        movementState = MovementState.Running;
    }
    #endregion

    private void Bounce()
    {
        rb.velocity = new Vector2(rb.velocity.x, bounceStrength);
        bouncing = true;
    }

    public float GetHorizontalVelocity()
    {
        return rb.velocity.x;
    }

    #region Hearts
    public void AddHeart(int score)
    {
        hearts = Mathf.Min(hearts + 1, maxHearts);

        if (score > 0)
        {
            scoreManager.AddScore(score);
        }

        UpdateHeartsDisplay();
    }

    private void ActivateHearts()
    {
        Vector2 capsuleCentre = activeCollider.bounds.center;
        Vector2 capsuleSize = (Vector2)activeCollider.bounds.size + (Vector2.one * heartActivationDistance);

        Collider2D[] hearts = Physics2D.OverlapCapsuleAll(capsuleCentre, capsuleSize, CapsuleDirection2D.Vertical, 0f, heartMask);

        for (int i = 0; i < hearts.Length; i++)
        {
            Heart heart = hearts[i].gameObject.GetComponent<Heart>();

            if (heart != null)
            {
                bool activated = heart.GetActivated();

                if (!activated)
                {
                    heart.Activate(this);
                }
            }
        }
    }

    private void SpawnHearts(int amount, Vector2 dropDirection)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, 360f / amount);

        for (int i = 0; i < amount; i++)
        {
            DroppedHeart heart = Instantiate(droppedHeart, activeCollider.bounds.center, Quaternion.identity).GetComponent<DroppedHeart>();
            heart.SetInitialVelocity(dropDirection * dropSpeed);

            dropDirection = rotation * dropDirection;
        }      
    }

    private void UpdateHeartsDisplay()
    {
        heartsCounter.text = hearts.ToString();
        heartsLevel.fillAmount = (float)hearts / (float)maxHearts;
    }
    #endregion

    #region Offscreen Checks
    public bool GetLyingDown()
    {
        bool lyingDown = false;

        switch (movementState)
        {
            case MovementState.Running:
                switch (runState)
                {
                    case RunState.Diving:
                        lyingDown = true;
                    break;
                }
                break;
            case MovementState.Crawling:
                    lyingDown = true;
                break;
            case MovementState.BellyFlopping:
                    lyingDown = true;
                break;
            case MovementState.Stunned:
                if (halfCollider.enabled)
                {
                    lyingDown = true;
                }
                break;
        }

        return lyingDown;
    }

    public Vector2 GetColliderBoundsMax()
    {
        return activeCollider.bounds.max;
    }   
    
    public Vector2 GetColliderBoundsMin()
    {
        return activeCollider.bounds.min;
    }

    public Vector2 GetSpriteRendererCentre()
    {
        return spriteRenderer.bounds.center;
    }

    public float GetSpriteRendererWidth()
    {
        return spriteRenderer.bounds.size.x;
    }
    #endregion

    public bool GetGrounded()
    {
        return grounded;
    }

    public float GetMaxRunSpeed()
    {
        return maxRunSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}