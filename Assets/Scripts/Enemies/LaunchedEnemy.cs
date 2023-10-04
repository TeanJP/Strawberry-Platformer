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

    [SerializeField]
    int health = 5;

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
            //If the launched enemy has been fired out of a cannon make it move and deal damage to anything in front of it.
            rb.velocity = direction * movementSpeed;
            Attack();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool platform = other.gameObject.CompareTag("Platform");
        bool breakable = other.gameObject.CompareTag("Breakable");

        //If the launched enemy collided with a platform destroy it unless it is the cannon that it was fired from.
        if ((platform || breakable) && other.gameObject != cannon && activated)
        {
            Crash();
        }
    }

    private void Attack()
    {
        Vector2 boxPosition = (Vector2)activeCollider.bounds.center + relativeBoxPosition;

        //Get all the targets of the launched enemy.
        Collider2D[] targets = Physics2D.OverlapBoxAll(boxPosition, boxSize, 0f, targetMask);

        //Loop through all the targets that were found.
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i].gameObject;

            bool player = false;

            if (target.transform.parent != null)
            {
                player = target.transform.parent.CompareTag("Player");
            }

            bool enemy = target.CompareTag("Enemy");
            bool launchedEnemy = target.CompareTag("Launched Enemy");

            Vector2 modifiedRepelDirection = repelDirection;

            //Set the repel direction of the launched enemy based on whether it is flying vertically or horizontally.
            if (direction.x == 0f)
            {
                modifiedRepelDirection.x *= Mathf.Sign(target.transform.position.x - activeCollider.bounds.center.x);
            }
            else
            {
                modifiedRepelDirection.x *= Mathf.Sign(direction.x);
            }

            //Deal damage to the target based on whether they were the player, an enemy or another launched enemy.
            if (player)
            {
                target.GetComponentInParent<Strawberry>().TakeDamge(damage, modifiedRepelDirection, repelStrength);
            }
            else if (enemy)
            {
                target.GetComponent<Enemy>().TakeDamage(false, lethalDamage, 0f, modifiedRepelDirection, repelStrength);
            }
            else if (launchedEnemy && target != gameObject)
            {
                target.GetComponent<LaunchedEnemy>().Crash();
                Crash();
            }
        }
    }

    public void SetDirection(Vector2 direction, GameObject cannon)
    {
        SetDirection(direction);

        this.cannon = cannon;

        activeCollider = GetComponent<BoxCollider2D>();

        transform.Rotate(0f, 0f, Vector2.SignedAngle(direction * new Vector2(-1f, 1f), Vector2.up));

        //Adjust the position of the attack hit box based on whether the launched enemy if travelling horizontally or vertically.
        if (transform.rotation.eulerAngles.z % 180f == 0f)
        {
            boxSize = new Vector2(activeCollider.bounds.size.x - attackReduction, attackWidth);
            relativeBoxPosition = new Vector2(0f, (activeCollider.bounds.extents.y - attackWidth * 0.5f) * Mathf.Sign(direction.y));
        }
        else
        {
            boxSize = new Vector2(attackWidth, activeCollider.bounds.size.x - attackReduction);
            relativeBoxPosition = new Vector2((activeCollider.bounds.extents.y - attackWidth * 0.5f) * Mathf.Sign(direction.x), 0f);
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

    public void TakeDamage(int damage)
    {
        health -= damage;

        //If the launched enemy has run out of health destroy it.
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
