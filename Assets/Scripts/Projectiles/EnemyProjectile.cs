using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb = null;

    private Vector2 direction = Vector2.right;
    [SerializeField]
    private float movementSpeed = 5f;

    [SerializeField]
    private int damage = 5;
    private Vector2 repelDirection = Vector2.one;
    private float repelStrength = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        repelDirection.Normalize();
    }

    void Update()
    {
        rb.velocity = direction * movementSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

        if (strawberry != null)
        {
            float horizontalDirection = Mathf.Sign(other.transform.position.x - transform.position.x);

            strawberry.TakeDamge(damage, repelDirection * new Vector2(horizontalDirection, 1f), repelStrength);
        }

        Destroy(gameObject);
    }
}
