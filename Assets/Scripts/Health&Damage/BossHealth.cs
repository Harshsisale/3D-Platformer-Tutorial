using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossHealth : Health
{
    [Tooltip("A list of events that occur when the health becomes 0 or lower")]
    public UnityEvent eventsOnDeath;

    protected override void Die()
    {
        // Do on death events
        if (eventsOnDeath != null)
        {
            eventsOnDeath.Invoke();
        }
        base.Die();
    }
}
