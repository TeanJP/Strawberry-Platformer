using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb = null;
    private BoxCollider2D activeCollider = null;

    private LayerMask platformMask;
    private float raycastLeniency = 0.02f;
    private float raycastLength = 0.02f;
    private int horizontalRaycasts = 2;
    private float horizontalRaycastSpacing;
    private int verticalRaycasts = 3;
    private float verticalRaycastSpacing;

    private float maxSpeed = 8f;
    private float initialSpeed = 4f;
    private float currentSpeed = 0f;
    private float acceleration = 5f;

    private int health = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        activeCollider = GetComponent<BoxCollider2D>();

        verticalRaycastSpacing = activeCollider.bounds.size.x / (verticalRaycasts - 1);
        horizontalRaycastSpacing = activeCollider.bounds.size.y / (horizontalRaycasts - 1);
    }

    void Update()
    {
        
    }

    #region Enemy Direction
    private void FlipDirection()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
    }

    private float GetFacingDirection()
    {
        return Mathf.Sign(transform.localScale.x);
    }
    #endregion

    #region Colision Checking
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
        Vector2 raycastDirection = new Vector2(GetFacingDirection(), 0f);
        Vector2 raycastOrigin = new Vector2(activeCollider.bounds.center.x + activeCollider.bounds.extents.x * GetFacingDirection(), activeCollider.bounds.min.y);

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
    #endregion

    public void TakeDamage(int damage)
    {
        health = Mathf.Max(health - damage, 0);

        if (health == 0)
        {
            Destroy(gameObject);
        }
    }
}
