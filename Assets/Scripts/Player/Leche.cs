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

    private Vector2 projectileOffset = new Vector2(0.75f, 0f);

    [SerializeField]
    private LayerMask platformMask;
    [SerializeField]
    private Vector2 maxOffset = new Vector2(1.25f, 0f);
    private Vector2 targetOffset;
    private Vector2 currentOffset;
    [SerializeField]
    private float movementSpeed = 2f;

    private SpriteRenderer spriteRenderer = null;
    private float halfSpriteWidth;
    private float raycastLength;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        halfSpriteWidth = spriteRenderer.bounds.extents.x;

        raycastLength = maxOffset.x + halfSpriteWidth;

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

        transform.localPosition = currentOffset * strawberry.GetPlayerDirection();
    }

    private void UpdateOffset()
    {
        float playerDirection = strawberry.GetPlayerDirection();

        Vector2 raycastDirection = new Vector2(playerDirection * -1f, 0f);

        float x = strawberry.GetCentre().x;
        float y;

        float verticalVelocity = strawberry.GetVerticalVelocity();

        if (verticalVelocity > 0f)
        {
            y = strawberry.GetTop();
        }
        else
        {
            y = strawberry.GetBase();
        }

        Vector2 raycastOrigin = new Vector2(x, y);

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

        if (hit.collider != null)
        {
            targetOffset.x = (hit.distance - halfSpriteWidth) * (playerDirection * -1f);
        }
        else
        {
            targetOffset.x = maxOffset.x * (playerDirection * -1f);
        }
    }

    private void UpdateDirection()
    {
        bool wallRunning = strawberry.GetWallRunning();
        float playerDirection = strawberry.GetPlayerDirection();
        float newDirection;

        if (wallRunning)
        {
            newDirection = playerDirection * -1f;
        }
        else
        {
            newDirection = playerDirection;
        }

        if (newDirection != Mathf.Sign(transform.localScale.x))
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
                    float horizontalDirection = Mathf.Sign(transform.localScale.x);
                    PlayerProjectile createdProjectile = Instantiate(projectile, (Vector2)transform.position + projectileOffset * horizontalDirection, Quaternion.identity).GetComponent<PlayerProjectile>();
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
