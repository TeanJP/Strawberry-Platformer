using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    void Start()
    {
        
    }

    public void Break()
    {
        Destroy(gameObject);
    }
}
