using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : EnemyProjectile
{
    [SerializeField]
    private float movementSpeed = 5f;

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        //Move in the provided direction.
        rb.velocity = direction * movementSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponentInParent<Strawberry>();

        if (strawberry != null)
        {
            //If the projectile hits the player damage them.
            float horizontalDirection = Mathf.Sign(other.transform.position.x - transform.position.x);

            strawberry.TakeDamge(damage, repelDirection * new Vector2(horizontalDirection, 1f), repelStrength);
        }

        //Destroy the projectile upon it hitting anything that it can collide with.
        Destroy(gameObject);
    }
}
