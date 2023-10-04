using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    protected Rigidbody2D rb = null;

    [SerializeField]
    private float initialSpeed = 6f;
    [SerializeField]
    private float maxSpeed = 32f;
    [SerializeField]
    private float acceleration = 10f;

    protected bool activated = false;

    private Strawberry strawberry = null;

    [SerializeField]
    private float targetLeniency = 0.02f;

    [SerializeField]
    private int score = 50;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (activated)
        {
            //Move towards the player.
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

            if (!stunned)
            {
                //If the player is not stunned pick up the heart.
                strawberry.AddHeart(score);
                Destroy(gameObject);
            }
        }
    }

    public bool GetActivated()
    {
        return activated;
    }

    public virtual void Activate(Strawberry strawberry)
    {
        this.strawberry = strawberry;
        activated = true;

        //Set the heart to move towards the player.
        rb.velocity = GetMovementDirection() * initialSpeed;
    }

    protected Vector2 GetMovementDirection()
    {
        //Adjust the direction of the heart to move towards that of the player.
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
