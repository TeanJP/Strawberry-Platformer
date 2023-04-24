using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingBlock : MonoBehaviour
{
    private enum State
    {
        Default,
        Disappearing,
        Reappearing
    }

    private State state = State.Default;

    private SpriteRenderer spriteRenderer = null;
    private BoxCollider2D blockCollider = null;

    [SerializeField]
    private LayerMask movingObjects;
    private Vector2 blockDimensions;

    [SerializeField]
    private float timeToDisappear = 2f;
    [SerializeField]
    private float timeToReappear = 3f;
    private float timer = 0f;
    

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        blockCollider = GetComponent<BoxCollider2D>();

        blockDimensions = blockCollider.bounds.size;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }

        switch (state)
        {
            case State.Disappearing:
                if (timer <= 0f)
                {
                    state = State.Reappearing;
                    timer = timeToReappear;

                    spriteRenderer.enabled = false;
                    blockCollider.enabled = false;
                }
                break;
            case State.Reappearing:
                if (timer <= 0f)
                {
                    Collider2D movingObject = Physics2D.OverlapBox(transform.position, blockDimensions, 0f, movingObjects);

                    if (movingObject == null)
                    {
                        state = State.Default;

                        spriteRenderer.enabled = true;
                        blockCollider.enabled = true;
                    }
                }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            bool playerBelowBlock = false;

            ContactPoint2D[] contacts = new ContactPoint2D[other.contactCount];
            other.GetContacts(contacts);

            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].normal.y >= 0f)
                {
                    playerBelowBlock = true;
                    break;
                }
            }

            if (!playerBelowBlock)
            {
                state = State.Disappearing;
                timer = timeToDisappear;
            }
        }
    }
}
