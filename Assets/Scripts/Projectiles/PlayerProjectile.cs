using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private enum State
    {
        Active,
        Destroyed,
        Fading
    }

    private State state = State.Active;

    private Rigidbody2D rb = null;
    private CircleCollider2D activeCollider = null;
    private SpriteRenderer spriteRenderer = null;
    private ParticleSystem sparkEffect = null;

    private Vector2 direction = Vector2.right;
    private float movementSpeed = 16f;

    [SerializeField]
    private int damage = 5;

    [SerializeField]
    private float lifeSpan = 5f;

    private Color initialColour;
    private Color currentColour;

    [SerializeField]
    private float fadeDuration = 0.5f;
    private float fadeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        activeCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        bool enemy = other.gameObject.CompareTag("Enemy");
        bool launchedEnemy = other.gameObject.CompareTag("Launched Enemy");

        //Deal damage to any enemy that is hit by the projectile.
        if (enemy)
        {
            Vector2 repelDirection = new Vector2(Mathf.Sign(other.transform.position.x - transform.position.x), 1f);
            repelDirection.Normalize();
            other.gameObject.GetComponent<Enemy>().TakeDamage(true, damage, 0f, repelDirection, 0f);
        }
        else if (launchedEnemy)
        {
            other.gameObject.GetComponent<LaunchedEnemy>().TakeDamage(damage);
        }

        state = State.Destroyed;

        ParticleSystem.MainModule mainModule = sparkEffect.main;
        mainModule.startColor = initialColour;
        sparkEffect.Play();

        spriteRenderer.enabled = false;
        activeCollider.enabled = false;

        rb.velocity = Vector2.zero;
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    public void SetMovementSpeed(float movementSpeed)
    {
        this.movementSpeed = movementSpeed;
    }
}
