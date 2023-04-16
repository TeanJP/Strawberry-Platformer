using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField]
    private float bounceStrength = 14f;

    void Start()
    {
        
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

        if (strawberry != null)
        {
            bool playerAbove = false;

            ContactPoint2D[] contacts = new ContactPoint2D[other.contactCount];
            other.GetContacts(contacts);

            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].normal.y < 0f)
                {
                    playerAbove = true;
                    break;
                }
            }

            if (playerAbove)
            {
                strawberry.Bounce(bounceStrength);
            }
        }
    }
}
