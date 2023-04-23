using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchedEnemy : EnemyProjectile
{
    [SerializeField]
    private float movementSpeed = 5f;

    [SerializeField]
    private LayerMask targetMask;

    private BoxCollider2D activeCollider = null;

    [SerializeField]
    private float attackWidth = 0.04f;
    [SerializeField]
    private float attackReduction = 0.02f;

    private Vector2 relativeBoxPosition;
    private Vector2 boxSize;

    private int lethalDamage = 100;

    private GameObject cannon = null;

    private bool activated = false;

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        if (activated)
        {
            rb.velocity = direction * movementSpeed;
            Attack();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool platform = other.gameObject.CompareTag("Platform");
        bool breakable = other.gameObject.CompareTag("Breakable");

        if ((platform || breakable) && other.gameObject != cannon && activated)
        {
            Crash();
        }
    }

    private void Attack()
    {
        Vector2 boxPosition = (Vector2)activeCollider.bounds.center + relativeBoxPosition;

        Collider2D[] targets = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, targetMask);

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;

            bool player = target.CompareTag("Player");
            bool enemy = target.CompareTag("Enemy");
            bool launchedEnemy = target.CompareTag("Launched Enemy");

            Vector2 modifiedRepelDirection = repelDirection;

            if (direction.x == 0f)
            {
                modifiedRepelDirection.x *= Mathf.Sign(target.transform.position.x - activeCollider.bounds.center.x);
            }
            else
            {
                modifiedRepelDirection.x *= Mathf.Sign(direction.x);
            }

            if (player)
            {
                target.GetComponent<Strawberry>().TakeDamge(damage, modifiedRepelDirection, repelStrength);
            }
            else if (enemy)
            {


                target.GetComponent<Enemy>().TakeDamage(false, lethalDamage, 0f, modifiedRepelDirection, repelStrength);
            }
            else if (launchedEnemy)
            {
                target.GetComponent<LaunchedEnemy>().Crash();
                Crash();
            }
        }
    }

    public void SetDirection(Vector2 direction, GameObject cannon)
    {
        base.SetDirection(direction);

        this.cannon = cannon;

        activeCollider = GetComponent<BoxCollider2D>();

        transform.Rotate(0f, 0f, Vector2.SignedAngle(direction, Vector2.up));

        if (transform.rotation.z % 180f == 0f)
        {
            boxSize = new Vector2(activeCollider.bounds.size.x - attackReduction, attackWidth);
            relativeBoxPosition = new Vector2(0f, (activeCollider.bounds.extents.y - attackWidth * 0.5f) * Mathf.Sign(direction.y));
        }
        else
        {
            boxSize = new Vector2(attackWidth, activeCollider.bounds.size.y - attackReduction);
            relativeBoxPosition = new Vector2((activeCollider.bounds.extents.x - attackWidth * 0.5f) * Mathf.Sign(direction.x), 0f);
        }
    }

    public Vector2 GetDirection()
    {
        return direction;
    }

    public void Crash()
    {
        Destroy(gameObject);
    }

    public void Activate()
    {
        activated = true;
    }
}
