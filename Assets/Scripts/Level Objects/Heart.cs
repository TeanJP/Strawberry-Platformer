using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    protected Rigidbody2D rb = null;

    [SerializeField]
    protected float movementSpeed = 8f;

    [SerializeField]
    protected float activationDistance = 2f;
    protected bool activated = false;

    [SerializeField]
    protected Transform strawberry = null;


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
