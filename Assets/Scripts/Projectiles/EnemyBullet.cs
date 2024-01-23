using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : EnemyProjectile
{
    private enum State
    {
        Active,
        Destroyed,
        Fading
    }

    private State state = State.Active;

    private CircleCollider2D activeCollider = null;
    private ParticleSystem sparkEffect = null;

    [SerializeField]
    private float movementSpeed = 5f;

    [SerializeField]
    private float lifeSpan = 5f;

    private Color initialColour;
    private Color currentColour;

    [SerializeField]
    private float fadeDuration = 0.5f;
    private float fadeTimer;

    protected override void Start()
    {
        base.Start();

        activeCollider = GetComponent<CircleCollider2D>();
        sparkEffect = GetComponent<ParticleSystem>();

        initialColour = spriteRenderer.color;
        currentColour = initialColour;
    }

    void Update()
    {
        switch (state)
        {
            case State.Active:
                lifeSpan -= Time.deltaTime;

                if (lifeSpan <= 0f)
                {
                    state = State.Fading;
                    fadeTimer = fadeDuration;
                }
                else
                {
                    //Move in the provided direction.
                    rb.velocity = direction * movementSpeed;
                }
                break;
            case State.Destroyed:
                if (sparkEffect.isStopped)
                {
                    Destroy(gameObject);
                }
                break;
            case State.Fading:
                fadeTimer -= Time.deltaTime;

                if (fadeTimer <= 0f)
                {
                    Destroy(gameObject);
                }
                else
                {
                    currentColour.a = Mathf.Lerp(0f, initialColour.a, fadeTimer / fadeDuration);
                    spriteRenderer.color = currentColour;
                }
                break;
        }
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
        state = State.Destroyed;

        sparkEffect.Play();

        spriteRenderer.enabled = false;
        activeCollider.enabled = false;

        rb.velocity = Vector2.zero;
    }
}
