using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyProjectile : MonoBehaviour
{
    protected Rigidbody2D rb = null;
    protected SpriteRenderer spriteRenderer = null;

    protected Vector2 direction = Vector2.right;

    [SerializeField]
    protected int damage = 5;
    protected Vector2 repelDirection = Vector2.one;
    protected float repelStrength = 3f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        repelDirection.Normalize();
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }
}
