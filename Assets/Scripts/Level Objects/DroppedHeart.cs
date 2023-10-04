using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedHeart : Heart
{
    [SerializeField]
    private float lifeSpanDuration = 8f;
    private float lifeSpanTimer = 0f;

    [SerializeField]
    private float groundedDrag = 3f;

    private CircleCollider2D physicsCollider = null;

    private Vector2 initialVelocity;

    protected override void Start()
    {
        base.Start();

        physicsCollider = transform.GetChild(0).GetComponent<CircleCollider2D>();

        lifeSpanTimer = lifeSpanDuration;

        rb.velocity = initialVelocity;
    }

    protected override void Update()
    {
        //If the dropped heart has not come in range of the player decrement it's duration.
        if (!activated)
        {
            lifeSpanTimer -= Time.deltaTime;

            //If the duration of the dropped heart has ended destroy it.
            if (lifeSpanTimer < 0f)
            {
                Destroy(gameObject);
            }
        }

        base.Update();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && !activated)
        {
            rb.drag = groundedDrag;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform") && !activated)
        {
            rb.drag = 0f;
        }
    }

    public override void Activate(Strawberry strawberry)
    {
        //Switch the heart to be kinematic and not use a collider so that it can fly towards the player.
        rb.bodyType = RigidbodyType2D.Kinematic;
        physicsCollider.enabled = false;

        rb.velocity = Vector2.zero;
        rb.drag = 0f;

        base.Activate(strawberry);
    }

    public void SetInitialVelocity(Vector2 initialVelocity)
    {
        this.initialVelocity = initialVelocity;
    }
}
