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
    private EdgeCollider2D blockCollider = null;

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
        blockCollider = GetComponent<EdgeCollider2D>();

        blockDimensions = spriteRenderer.bounds.size;
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

                    //Make the block invisible and prevent it being collided with.
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
                        //If there is no object in the way of where the block should be make it reappear.
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
                    //Get whether the player hit the bottom of the block.
                    playerBelowBlock = true;
                    break;
                }
            }

            if (!playerBelowBlock)
            {
                //If the player did not hit the bottom of the block set it to disappear.
                state = State.Disappearing;
                timer = timeToDisappear;
            }
        }
    }
}
