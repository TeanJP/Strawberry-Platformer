using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strawberry : MonoBehaviour
{
    private Rigidbody2D rb = null;
    private SpriteRenderer spriteRenderer = null;
    private Vector2 spriteDimensions;

    [SerializeField]
    private LayerMask platformMask;

    [SerializeField]
    float walkSpeed = 5f;

    Vector2 movement = Vector2.zero;

    [SerializeField]
    private float fallSpeed = 2.5f;
    [SerializeField]
    private float lowJumpSpeed = 2f;

    [SerializeField]
    private float jumpStrength = 6f;
    private bool holdingJump = false;

    [SerializeField]
    private float inputBuffer = 0.2f;
    private float inputTimer = 0f;

    [SerializeField]
    private float groundedBuffer = 0.15f;
    private float groundedTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteDimensions = spriteRenderer.bounds.extents;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Sign(horizontalInput) != Mathf.Sign(transform.localScale.x) && horizontalInput != 0f)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
        }

        movement.x = horizontalInput * walkSpeed;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            holdingJump = true;
            inputTimer = inputBuffer;
        }
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            holdingJump = false;
        }
    }

    void FixedUpdate()
    {
        bool grounded = GetGrounded();

        if (grounded)
        {
            groundedTimer = groundedBuffer;
        }

        Debug.Log(groundedTimer + " " + inputTimer);

        movement.y = rb.velocity.y;

        if (rb.velocity.y < 0f)
        {
            rb.gravityScale = fallSpeed;
        }
        else if ((rb.velocity.y > 0f) && !holdingJump)
        {
            rb.gravityScale = lowJumpSpeed;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        if (inputTimer > 0f && groundedTimer > 0f)
        {
            movement.y = jumpStrength;
            inputTimer = 0f;
        }

        rb.velocity = movement;

        if (groundedTimer > 0f)
        {
            groundedTimer -= Time.fixedDeltaTime;
        }

        if (inputTimer > 0f)
        {
            inputTimer -= Time.fixedDeltaTime;
        }
    }

    private bool GetGrounded()
    {
        float boxHeight = 0.01f;
        Vector2 boxCheckPosition =  new Vector2(transform.position.x, transform.position.y - spriteDimensions.y - boxHeight * 0.5f);
        Vector2 boxCheckSize = new Vector2(spriteDimensions.x * 2f, boxHeight);

        Collider2D[] platforms = Physics2D.OverlapBoxAll(boxCheckPosition, boxCheckSize, 0f, platformMask);

        if (platforms.Length != 0)
        {
            for (int i = 0; i < platforms.Length; i++)
            {
                if ((platforms[i].transform.position.y + platforms[i].bounds.extents.y) < transform.position.y)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool GetOnWall()
    {
        return false;
    }
}
