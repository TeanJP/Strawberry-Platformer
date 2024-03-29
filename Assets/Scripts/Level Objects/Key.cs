using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private Rigidbody2D rb = null;
    private BoxCollider2D activeCollider = null;

    [SerializeField]
    private LayerMask playerMask;

    private bool activated = false;
    private bool visible = false;

    private Strawberry strawberry = null;

    [SerializeField]
    private float initialSpeed = 6f;
    [SerializeField]
    private float maxSpeed = 32f;
    [SerializeField]
    private float acceleration = 10f;

    [SerializeField]
    private float targetLeniency = 0.02f;

    [SerializeField]
    private float activationDistance = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        activeCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (visible && !activated)
        {
            Vector2 capsuleCentre = activeCollider.bounds.center;
            Vector2 capsuleSize = (Vector2)activeCollider.bounds.size + (Vector2.one * activationDistance);

            Collider2D other = Physics2D.OverlapCapsule(capsuleCentre, capsuleSize, CapsuleDirection2D.Vertical, 0f, playerMask);

            //If the player is within range of the key.
            if (other != null)
            {
                Strawberry strawberry = other.GetComponent<Strawberry>();

                if (strawberry != null)
                {
                    //If the player is not stunned.
                    if (!strawberry.GetStunned())
                    {
                        //Activate the key.
                        this.strawberry = strawberry;
                        activated = true;
                      
                        rb.velocity = GetMovementDirection() * initialSpeed;
                    }
                }
            }
        }
        else if (activated)
        {
            //Move towards the player if the key is activated.
            Vector2 movementDirection = GetMovementDirection();
            Vector2 movement = rb.velocity + (movementDirection * acceleration * Time.deltaTime);

            movement = Vector2.ClampMagnitude(movement, maxSpeed);

            rb.velocity = movement;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

        if (strawberry != null)
        {
            bool stunned = strawberry.GetStunned();

            //If the player collided with the key and were not stunned set the game as in the escape phase.
            if (!stunned)
            {
                GameManager.Instance.StartEscape();
                Destroy(gameObject);
            }
        }
    }

    void OnBecameVisible()
    {
        visible = true;
    }

    void OnBecameInvisible()
    {
        visible = false;
    }

    private Vector2 GetMovementDirection()
    {
        //Adjust the direction of the key to move towards that of the player.
        Vector2 currentDirection = rb.velocity.normalized;
        Vector2 targetDirection = strawberry.GetCentre() - (Vector2)transform.position;
        targetDirection.Normalize();

        Vector2 difference = targetDirection - currentDirection;

        if (difference.magnitude < targetLeniency)
        {
            //If the difference between the direction to the player and the current direction is small enough don't change the direction.
            return currentDirection;
        }
        else
        {
            return difference.normalized;
        }
    }
}
