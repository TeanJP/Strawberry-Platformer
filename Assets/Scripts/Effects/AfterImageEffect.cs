using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageEffect : MonoBehaviour
{
    [SerializeField]
    private GameObject afterImage = null;
    [SerializeField]
    private int afterImageCount = 6;

    private Queue<AfterImage> afterImages = new Queue<AfterImage>();

    [SerializeField]
    private List<Color> colours = new List<Color>();
    private int currentColour = 0;

    private SpriteRenderer targetSpriteRenderer = null;

    [SerializeField]
    private float afterImageDuration = 0.25f;

    [SerializeField]
    private float delayDuration = 0.5f;
    private float delayTimer = 0f;

    void Start()
    {
        Transform afterImageParent = new GameObject(gameObject.name + " After Images").transform; 

        for (int i = 0; i < afterImageCount; i++)
        {
            AfterImage afterImageInstance = Instantiate(afterImage).GetComponent<AfterImage>();
            afterImageInstance.transform.parent = afterImageParent;

            afterImages.Enqueue(afterImageInstance);
        }

        targetSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
    }

    public void Display(float horizontalDirection)
    {
        if (delayTimer <= 0f)
        {
            if (!afterImages.Peek().GetActive())
            {
                AfterImage afterImageInstance = afterImages.Dequeue();
                afterImages.Enqueue(afterImageInstance);

                afterImageInstance.Display(targetSpriteRenderer.transform.position, horizontalDirection, targetSpriteRenderer.sprite, colours[currentColour], afterImageDuration);

                currentColour++;

                if (currentColour >= colours.Count)
                {
                    currentColour = 0;
                }

                delayTimer = delayDuration;
            }
        }
    }
}
