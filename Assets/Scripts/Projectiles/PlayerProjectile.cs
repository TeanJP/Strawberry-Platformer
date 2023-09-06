using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Rigidbody2D rb = null;

    private Vector2 direction = Vector2.right;
    [SerializeField]
    private float movementSpeed = 10f;

    [SerializeField]
    private int damage = 5;

    [SerializeField]
    private float lifeSpan = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeSpan);
    }

    void Update()
    {
        rb.velocity = direction * movementSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool enemy = other.gameObject.CompareTag("Enemy");
        bool launchedEnemy = other.gameObject.CompareTag("Launched Enemy");

        if (enemy)
        {
            Vector2 repelDirection = new Vector2(Mathf.Sign(other.transform.position.x - transform.position.x), 1f);
            repelDirection.Normalize();
            other.gameObject.GetComponent<Enemy>().TakeDamage(true, damage, 0f, repelDirection, 0f);
        }
        else if (launchedEnemy)
        {
            //Maybe add knockback for when the enemy is defeated.
            other.gameObject.GetComponent<LaunchedEnemy>().TakeDamage(damage);
        }

        Destroy(gameObject);      
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }
}
