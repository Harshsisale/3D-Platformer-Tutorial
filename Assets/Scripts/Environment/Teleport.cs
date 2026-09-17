using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The destination object with a Teleport script attached.")]
    public Teleport destinationTeleporter;

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        
            // reposition the player
            Debug.Log(other.name + " collided with " + this.transform.parent.name + " so teleport to " + destinationTeleporter.transform.parent.name);
    }

    // OnTriggerExit is called when the Collider other has stopped touching the trigger
    private void OnTriggerExit(Collider other)
    {
    }
}