using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    private SpriteRenderer spriteRenderer = null;
    private EdgeCollider2D activeCollider = null;
    private ParticleSystem debrisEffect = null;

    private bool destroyed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        activeCollider = GetComponent<EdgeCollider2D>();
        debrisEffect = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (destroyed)
        {
            if (debrisEffect.isStopped)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Break()
    {
        destroyed = true;

        spriteRenderer.enabled = false;
        activeCollider.enabled = false;

        debrisEffect.Play();
    }
}
