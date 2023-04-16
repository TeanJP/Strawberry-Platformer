using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField]
    private int damage = 20;
    [SerializeField]
    protected Vector2 repelDirection = Vector2.one;
    [SerializeField]
    protected float repelStrength = 3f;

    void Start()
    {
        repelDirection.Normalize();

        if (transform.rotation.z % 360f != 0f)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, transform.rotation.z);

            Vector2 rotatedRepelDirection = rotation * repelDirection;

            if (transform.rotation.z % 180f == 0f)
            {
                rotatedRepelDirection.x = Mathf.Abs(rotatedRepelDirection.x);
            }
            else if (transform.rotation.z % 90f == 0f)
            {
                rotatedRepelDirection.y = Mathf.Abs(rotatedRepelDirection.y);
            }

            repelDirection = rotatedRepelDirection;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

        if (strawberry != null)
        {
            Vector2 collisionNormal = other.GetContact(0).normal;
            Vector2 modifiedRepelDirection = GetRepelDirection(strawberry, collisionNormal);

            strawberry.TakeDamge(damage, modifiedRepelDirection, repelStrength);     
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

        if (strawberry != null)
        {
            Vector2 collisionNormal = other.GetContact(0).normal;
            Vector2 modifiedRepelDirection = GetRepelDirection(strawberry, collisionNormal);

            strawberry.TakeDamge(damage, modifiedRepelDirection, repelStrength);
        }
        
    }

    private Vector2 GetRepelDirection(Strawberry strawberry, Vector2 collisionNormal)
    {
        Vector2 modifiedRepelDirection = repelDirection;

        if (collisionNormal.x == 0f)
        {
            float horizontalVelocity = strawberry.GetHorizontalVelocity();

            if (horizontalVelocity == 0f)
            {
                modifiedRepelDirection.x = 0f;
                modifiedRepelDirection.Normalize();
            }
            else
            {
                modifiedRepelDirection.x *= Mathf.Sign(horizontalVelocity);
            }

            modifiedRepelDirection.y *= (collisionNormal.y * -1f);
        }
        else
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, 90f);
            
            modifiedRepelDirection = rotation * modifiedRepelDirection;
            modifiedRepelDirection.y = Mathf.Abs(modifiedRepelDirection.y);
            modifiedRepelDirection.x *= collisionNormal.x;
        }

        return modifiedRepelDirection;
    }
}
