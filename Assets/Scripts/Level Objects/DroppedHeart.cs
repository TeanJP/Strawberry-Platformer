using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedHeart : Heart
{
    [SerializeField]
    private float lifeSpanDuration = 8f;
    private float lifeSpanTimer = 0f;

    private CircleCollider2D physicsCollider = null;

    protected override void Start()
    {
        base.Start();

        physicsCollider = transform.GetChild(0).GetComponent<CircleCollider2D>();

        lifeSpanTimer = lifeSpanDuration;
    }

    protected override void Update()
    {
        if (!activated)
        {
            lifeSpanTimer -= Time.deltaTime;

            if (lifeSpanTimer < 0f)
            {
                Destroy(gameObject);
            }
        }

        base.Update();
    }

    public override void Activate(Strawberry strawberry)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        physicsCollider.enabled = false;

        base.Activate(strawberry);
    }

    public void SetInitialVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }
}
