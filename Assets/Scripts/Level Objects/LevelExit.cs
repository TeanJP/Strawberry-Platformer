using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour
{
    private GameManager gameManager = null;

    private BoxCollider2D exitTrigger = null;
    private SpriteRenderer spriteRenderer = null;

    [SerializeField]
    private Color closedColour = Color.gray;
    [SerializeField]
    private Color openColour = Color.white;

    void Start()
    {
        gameManager = GameManager.Instance;

        exitTrigger = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        exitTrigger.enabled = false;
        spriteRenderer.color = closedColour;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!gameManager.GetGamePaused() && !gameManager.GetGameWon())
        {
            Strawberry strawberry = other.GetComponent<Strawberry>();

            if (strawberry != null)
            {
                if (!strawberry.GetStunned() && strawberry.GetGrounded() && Input.GetKey(KeyCode.UpArrow))
                {
                    gameManager.SetGameWon();
                }
            }
        }
    }

    public void SetOpen()
    {
        exitTrigger.enabled = true;
        spriteRenderer.color = openColour;
    }
}
