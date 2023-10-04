using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField]
    private int playerDamage = 20;
    [SerializeField]
    private int enemyDamage = 100;
    [SerializeField]
    private float enemyStunDuration = 3f;
    [SerializeField]
    protected Vector2 repelDirection = Vector2.one;
    [SerializeField]
    protected float repelStrength = 3f;

    void Start()
    {
        repelDirection.Normalize();

        //Adjust the repel direction of the spike based on it's rotation.
        if (transform.rotation.eulerAngles.z % 360f != 0f)
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, transform.rotation.z);

            Vector2 rotatedRepelDirection = rotation * repelDirection;

            if (transform.rotation.eulerAngles.z % 180f == 0f)
            {
                rotatedRepelDirection.x = Mathf.Abs(rotatedRepelDirection.x);
            }
            else if (transform.rotation.eulerAngles.z % 90f == 0f)
            {
                rotatedRepelDirection.y = Mathf.Abs(rotatedRepelDirection.y);
            }

            repelDirection = rotatedRepelDirection;
        }
    }

    #region Collision
    void OnCollisionEnter2D(Collision2D other)
    {
        bool isPlayer = other.gameObject.CompareTag("Player");
        bool isEnemy = other.gameObject.CompareTag("Enemy");

        Vector2 collisionNormal, modifiedRepelDirection;   

        if (isPlayer)
        {
            Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

            if (strawberry != null)
            {
                //If the player hit the spike damage them.
                collisionNormal = other.GetContact(0).normal;
                modifiedRepelDirection = GetRepelDirection(strawberry, collisionNormal);

                strawberry.TakeDamge(playerDamage, modifiedRepelDirection, repelStrength);
            }
        }
        else if (isEnemy)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                //If an enemy hit the spike damage them.
                if (enemy.GetVisible())
                {
                    collisionNormal = other.GetContact(0).normal;
                    modifiedRepelDirection = GetRepelDirection(enemy, collisionNormal);

                    enemy.TakeDamage(false, enemyDamage, enemyStunDuration, modifiedRepelDirection, repelStrength);
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        //Damage the player or enemy if they couldn't be damaged upon entering a collision with the spike.
        bool isPlayer = other.gameObject.CompareTag("Player");
        bool isEnemy = other.gameObject.CompareTag("Enemy");

        Vector2 collisionNormal, modifiedRepelDirection;

        if (isPlayer)
        {
            Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

            //If the player hit the spike damage them.
            if (strawberry != null)
            {
                collisionNormal = other.GetContact(0).normal;
                modifiedRepelDirection = GetRepelDirection(strawberry, collisionNormal);

                strawberry.TakeDamge(playerDamage, modifiedRepelDirection, repelStrength);
            }
        }
        else if (isEnemy)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            //If an enemy hit the spike damage them.
            if (enemy != null)
            {
                if (enemy.GetVisible())
                {
                    collisionNormal = other.GetContact(0).normal;
                    modifiedRepelDirection = GetRepelDirection(enemy, collisionNormal);

                    enemy.TakeDamage(false, enemyDamage, enemyStunDuration, modifiedRepelDirection, repelStrength);
                }
            }
        }
    }
    #endregion

    #region Repel Direction
    private Vector2 GetRepelDirection(Strawberry strawberry, Vector2 collisionNormal)
    {
        Vector2 modifiedRepelDirection = repelDirection;

        if (collisionNormal.x == 0f)
        {
            //If the collision was vertical.
            float horizontalVelocity = strawberry.GetHorizontalVelocity();

            if (horizontalVelocity == 0f)
            {
                //If the player is not moving only repel them vertically.
                modifiedRepelDirection.x = 0f;
                modifiedRepelDirection.Normalize();
            }
            else
            {
                //Use the player's velocity to set the direction of the repel force.
                modifiedRepelDirection.x *= Mathf.Sign(horizontalVelocity);
            }

            modifiedRepelDirection.y *= (collisionNormal.y * -1f);
        }
        else
        {
            //If the collision was horizontal.
            Quaternion rotation = Quaternion.Euler(0f, 0f, 90f);
            
            //Rotate the repel direction to be horizontal.
            modifiedRepelDirection = rotation * modifiedRepelDirection;
            modifiedRepelDirection.y = Mathf.Abs(modifiedRepelDirection.y);
            modifiedRepelDirection.x *= collisionNormal.x;
        }

        return modifiedRepelDirection;
    }

    private Vector2 GetRepelDirection(Enemy enemy, Vector2 collisionNormal)
    {
        Vector2 modifiedRepelDirection = repelDirection;

        if (collisionNormal.x == 0f)
        {
            //If the collision was vertical.
            float horizontalVelocity = enemy.GetHorizontalVelocity();

            if (horizontalVelocity == 0f)
            {
                //If the enemy is not moving only repel them vertically.
                modifiedRepelDirection.x = 0f;
                modifiedRepelDirection.Normalize();
            }
            else
            {
                //Use the enemy's velocity to set the direction of the repel force.
                modifiedRepelDirection.x *= Mathf.Sign(horizontalVelocity);
            }

            modifiedRepelDirection.y *= (collisionNormal.y * -1f);
        }
        else
        {
            //If the collision was horizontal.
            Quaternion rotation = Quaternion.Euler(0f, 0f, 90f);

            //Rotate the repel direction to be horizontal.
            modifiedRepelDirection = rotation * modifiedRepelDirection;
            modifiedRepelDirection.y = Mathf.Abs(modifiedRepelDirection.y);
            modifiedRepelDirection.x *= collisionNormal.x;
        }

        return modifiedRepelDirection;
    }
    #endregion
}
