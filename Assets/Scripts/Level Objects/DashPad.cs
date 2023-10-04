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
        //If the speed booster has not been used yet.
        if (!used)
        {
            Strawberry strawberry = other.gameObject.GetComponent<Strawberry>();

            if (strawberry != null)
            {
                bool canAddSpeed = strawberry.GetCanAddSpeed();

                if (canAddSpeed)
                {
                    //Add speed to the player.
                    strawberry.AddSpeed(direction, speed);
                    used = true;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //When the player moves out of the speed booster set it as not being used.
        if (other.gameObject.layer == player)
        {
            used = false;
        }
    }
}
