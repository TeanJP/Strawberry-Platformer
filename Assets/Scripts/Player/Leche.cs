using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leche : MonoBehaviour
{
    [SerializeField]
    private Strawberry strawberry = null;

    [SerializeField]
    private GameObject projectile = null;

    [SerializeField]
    private float attackDelay = 0.25f;
    private float attackTimer = 0f;

    [SerializeField]
    private Vector2 projectileOffset = new Vector2(0.25f, 0f);

    [SerializeField]
    private LayerMask platformMask;
    [SerializeField]
    private Vector2 maxOffset = new Vector2(1.25f, 0f);
    private Vector2 targetOffset;
    private Vector2 currentOffset;
    [SerializeField]
    private float movementSpeed = 2f;

    [SerializeField]
    private float halfColliderOffset = -0.25f;

    private SpriteRenderer spriteRenderer = null;
    private Animator animator = null;

    private Vector2 halfDimensions;
    private float raycastLength;

    [SerializeField]
    private Vector2 raycastOffset = new Vector2(0f, 0.18f);
    [SerializeField]
    private float raycastSpacing = 1f;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        
        animator = gameObject.GetComponent<Animator>();
        animator.Play("Leche Default");

        halfDimensions = spriteRenderer.bounds.extents;

        raycastLength = maxOffset.x + halfDimensions.x;

        UpdateDirection();
        targetOffset = maxOffset * new Vector2(strawberry.GetPlayerDirection() * -1f, 1f);

        currentOffset = targetOffset;
    }

    void LateUpdate()
    {
        UpdateDirection();
        UpdateOffset();
        Move(Time.deltaTime);

        if (Input.GetKey(KeyCode.C))
        {
            Attack();
        }
    }

    private void Move(float deltaTime)
    {
        if (currentOffset != targetOffset)
        {
            if (currentOffset.x < targetOffset.x)
            {
                currentOffset.x = Mathf.Min(currentOffset.x + movementSpeed * deltaTime, targetOffset.x);
            }
            else
            {
                currentOffset.x = Mathf.Max(currentOffset.x - movementSpeed * deltaTime, targetOffset.x);
            }
        }

        bool usingHalfCollider = strawberry.GetUsingHalfCollider();
        
        if (usingHalfCollider)
        {
            currentOffset.y = halfColliderOffset;
        }
        else
        {
            currentOffset.y = 0f;
        }

        float playerDirection = strawberry.GetPlayerDirection();
        transform.localPosition = currentOffset * new Vector2(playerDirection * -1f, 1f);
    }

    private void UpdateOffset()
    {
        float playerDirection = strawberry.GetPlayerDirection();

        Vector2 raycastDirection = new Vector2(playerDirection * -1f, 0f);

        Vector2 raycastOrigin = new Vector2(strawberry.GetCentre().x, spriteRenderer.bounds.center.y) + raycastOffset;
        Vector2 offset = new Vector2(0f, raycastSpacing * 0.5f * Mathf.Sign(strawberry.GetVerticalVelocity()));

        for (int i = 0; i < 2; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin + offset, raycastDirection, raycastLength, platformMask);

            if (hit.collider != null)
            {
                targetOffset.x = Mathf.Abs(hit.distance - halfDimensions.x) * (playerDirection * -1f);
                return;
            }

            offset *= -1f;
        }

        targetOffset.x = maxOffset.x * (playerDirection * -1f);
    }

    private float GetLecheDirection()
    {
        return strawberry.GetPlayerDirection() * Mathf.Sign(transform.localScale.x);
    }

    private void UpdateDirection()
    {
        bool wallRunning = strawberry.GetWallRunning();
        
        if ((wallRunning && Mathf.Sign(transform.localScale.x) != -1f) || (!wallRunning && Mathf.Sign(transform.localScale.x) != 1f))
        {
            transform.localScale *= new Vector2(-1f, 1f);
        }
    }

    private void Attack()
    {
        bool playerStunned = strawberry.GetStunned();

        if (!playerStunned)
        {
            if (attackTimer <= 0f)
            {
                if (projectile != null)
                {
                    float horizontalDirection = GetLecheDirection();
                    PlayerProjectile createdProjectile = Instantiate(projectile, (Vector2)transform.position + projectileOffset * new Vector2 (horizontalDirection, 1f), Quaternion.identity).GetComponent<PlayerProjectile>();
                    createdProjectile.SetDirection(new Vector2(horizontalDirection, 0f));
                }

                attackTimer = attackDelay;
            }

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
        }
    }
}
