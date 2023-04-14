using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShockwave : EnemyProjectile
{
    [SerializeField]
    private float shockwaveDuration = 1f;
    private float shockwaveTimer;

    private float movementStep;

    [SerializeField]
    private LayerMask platformMask;

    [SerializeField]
    private float raycastLength = 0.02f;
    private int horizontalRaycasts = 2;
    private float horizontalRaycastSpacing;

    protected override void Start()
    {
        base.Start();

        shockwaveTimer = shockwaveDuration;

        movementStep = spriteRenderer.bounds.extents.x;

        horizontalRaycastSpacing = spriteRenderer.bounds.size.y / (horizontalRaycasts - 1);
    }

    void Update()
    {
        if (shockwaveTimer < 0f)
        {
            Vector2 targetPosition = rb.position + direction * movementStep;

            bool dropAtPosition = GetDropAtPosition(targetPosition);
            bool wallAhead = GetWallAhead();

            if (!dropAtPosition && !wallAhead)
            {
                rb.MovePosition(targetPosition);
                shockwaveTimer = shockwaveDuration;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            shockwaveTimer -= Time.deltaTime;
        }
    }

    #region Strawberry Collision
    void OnTriggerEnter2D(Collider2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponentInParent<Strawberry>();

        if (strawberry != null)
        {
            float horizontalDirection = Mathf.Sign(other.transform.position.x - transform.position.x);
            strawberry.TakeDamge(damage, repelDirection * new Vector2(horizontalDirection, 1f), repelStrength);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponentInParent<Strawberry>();

        if (strawberry != null)
        {
            float horizontalDirection = Mathf.Sign(other.transform.position.x - transform.position.x);
            strawberry.TakeDamge(damage, repelDirection * new Vector2(horizontalDirection, 1f), repelStrength);
        }
    }
    #endregion

    #region Collision Checks
    private bool GetDropAtPosition(Vector2 position)
    {
        Vector2 raycastDirection = Vector2.down;
        Vector2 raycastOrigin = new Vector2(position.x, position.y - spriteRenderer.bounds.extents.y);

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, raycastLength, platformMask);

        return hit.collider == null;
    }

    private bool GetWallAhead()
    {
        Vector2 raycastDirection = new Vector2(Mathf.Sign(direction.x), 0f);
        Vector2 raycastOrigin = new Vector2(spriteRenderer.bounds.center.x + spriteRenderer.bounds.extents.x * raycastDirection.x, spriteRenderer.bounds.min.y);

        for (int i = 0; i < horizontalRaycasts; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, movementStep, platformMask);

            if (hit.collider != null)
            {
                return true;
            }

            raycastOrigin.y += horizontalRaycastSpacing;
        }

        return false;
    }
    #endregion
}
