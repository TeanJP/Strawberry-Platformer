using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    protected Rigidbody2D rb = null;

    [SerializeField]
    protected float maxSpeed = 4f;
    [SerializeField]
    protected float acceleration = 6f;

    protected bool activated = false;

    protected Transform strawberry = null;


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (activated)
        {
            Vector2 movementDirection = GetMovementDirection();            
            Vector2 movement = rb.velocity + (movementDirection * acceleration * Time.deltaTime);

            movement = Vector2.ClampMagnitude(movement, maxSpeed);

            rb.velocity = movement;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

        if (strawberry != null)
        {
            strawberry.AddHeart();
            Destroy(gameObject);
        }
    }

    public bool GetActivated()
    {
        return activated;
    }

    public virtual void Activate(Transform strawberry)
    {
        this.strawberry = strawberry;
        activated = true;
    }

    protected Vector2 GetMovementDirection()
    {
        Vector2 currentDirection = rb.velocity.normalized;
        Vector2 targetDirection = strawberry.position - transform.position;
        targetDirection.Normalize();

        Vector2 movementDirection = targetDirection - currentDirection;
        movementDirection.Normalize();

        return movementDirection;
    }
}
