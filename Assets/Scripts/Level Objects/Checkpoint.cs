using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private Sprite activatedSprite = null;
    private bool activated = false;

    private GameManager gameManager = null;
    private CheckpointManager checkpointManager = null;

    private ParticleSystem starEffect = null;

    void Start()
    {
        gameManager = GameManager.Instance;
        checkpointManager = gameManager.GetCheckpointManager();

        if (starEffect == null)
        {
            SetStarEffect();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //If the game is not in the escape phase.
        if (!gameManager.GetEscapeActive())
        {
            bool player = other.gameObject.CompareTag("Player");

            if (player)
            {
                //If the player collided with the checkpoint set the current checkpoint to this checkpoint.
                checkpointManager.SetCurrentCheckpoint(this);

                if (!activated)
                {
                    ActivateCheckpoint();
                }
            }
        }
    }

    public void SetStarEffect()
    {
        starEffect = GetComponent<ParticleSystem>();
    }

    public void ActivateCheckpoint()
    {
        activated = true;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = activatedSprite;

        starEffect.Play();
    }
}
