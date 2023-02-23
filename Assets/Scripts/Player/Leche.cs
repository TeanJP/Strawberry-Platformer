using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leche : MonoBehaviour
{
    private Rigidbody2D rb = null;

    [SerializeField]
    private Strawberry strawberry = null;

    [SerializeField]
    private GameObject projectile = null;

    [SerializeField]
    private float attackDelay = 0.25f;
    private float attackTimer = 0f;

    [SerializeField]
    private float maxDistance = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C) && attackTimer <= 0f)
        {
            Projectile createdProjectile = Instantiate(projectile).GetComponent<Projectile>();

            attackTimer = attackDelay;    
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        Vector2 directionToStrawberry = strawberry.transform.position - transform.position;
        float distanceFromStrawberry = directionToStrawberry.magnitude;

        if (distanceFromStrawberry > maxDistance)
        {

        }
    }
}
