using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null;

    private float duration;
    private float timer = 0f;

    private float initialOpacity;

    private Color colour;

    private Vector2 position;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;        
    }

    void Update()
    {
        if (timer > 0f)
        {
            transform.position = this.position;

            colour.a = Mathf.Lerp(0f, initialOpacity, timer / duration);
            spriteRenderer.color = colour;

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                spriteRenderer.enabled = false;
            }
        }
    }

    public void Display(Vector2 position, float horizontalDirection, Sprite sprite, Color colour, float duration)
    {
        this.position = position;
        transform.position = this.position;

        transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * horizontalDirection, transform.localScale.y);

        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite;

        initialOpacity = colour.a;

        this.colour = colour;
        spriteRenderer.color = this.colour;

        this.duration = duration;
        timer = this.duration;
    }

    public bool GetActive()
    {
        return timer > 0f;
    }
}
