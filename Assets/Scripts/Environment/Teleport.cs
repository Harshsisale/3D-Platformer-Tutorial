using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The destination object with a Teleport script attached.")]
    public Teleport destinationTeleporter;

    // teleporterAvailable tracks the state of the teleporter. It is used to turn off the destination Teleporter just before the player is moved to it so the player does not teleport back immediately.
    private bool teleporterAvailable = true;

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other)
    {
      if ((other.tag == "Player") && (teleporterAvailable==true))
      {
            Debug.Log(other.name + " collided with " + this.transform.parent.name + " so teleport to " + destinationTeleporter.transform.parent.name);
            // turn off destination teleporter so it will not teleport player right back
            destinationTeleporter.teleporterAvailable = false;
            // reposition the player
            other.transform.position = destinationTeleporter.transform.position
      }
    }

    // OnTriggerExit is called when the Collider other has stopped touching the trigger
    private void OnTriggerExit(Collider other)
    {
      if (other.tag == "Player")
      {
        // when player exits the teleporter, make it available again for next time they enter
        teleporterAvailable = true;
      }
    }
}