using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeacefulEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        UpdateState();

        switch (state)
        {
            case State.Default:

                break;
            case State.Scared:

                break;
            case State.Stunned:

                break;
        }
    }

    private void UpdateState()
    {
        switch (state)
        {
            case State.Default:

                break;
            case State.Attacking:

                break;
            case State.Scared:

                break;
            case State.Stunned:

                break;
        }
    }
}
