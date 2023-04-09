using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPad : MonoBehaviour
{
    private int player = 7;

    private bool used = false;

    [SerializeField]
    private float speed = 8f;
    private float direction;

    void Start()
    {
        direction = Mathf.Sign(transform.localScale.x);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!used)
        {
            Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

            if (strawberry != null)
            {
                bool canAddSpeed = strawberry.GetCanAddSpeed();

                if (canAddSpeed)
                {
                    strawberry.AddSpeed(direction, speed);
                    used = true;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == player)
        {
            used = false;
        }
    }
}
