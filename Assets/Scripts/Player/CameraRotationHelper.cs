using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is meant to be applied to the parent of the player camera game object.
/// 
/// This script makes the attached gameobject follow the player's transform without changing its own rotation
/// Then the camera rotates the gameobject this script is attached to to make the camera rotate around the horizontal and vertical smoothly
/// </summary>
public class CameraRotationHelper : MonoBehaviour {

    // The camera that should be a child of this gameobject
    GameObject childCamera;

    /// <summary>
    /// Description:
    /// Standard Unity function that is called before the first frame
    /// Input:
    /// none
    /// Return:
    /// void
    /// </summary>
    void Start()
    {
        InitialSetup();
    }

    /// <summary>
    /// Description:
    /// Checks for and gets the needed refrences for this script to run correctly
    /// Also sets up anything else that needs to be done initially
    /// Input:
    /// none
    /// Return:
    /// void
    /// </summary>
    void InitialSetup()
    {
        childCamera = this.gameObject.GetComponentInChildren<Camera>().gameObject;
        if (childCamera == null)
        {
            Debug.LogError("There is not a camera childed underneath the CameraRotationHelper script game object " +
                "\n The camera game object should be a child of the camera rotation helper or it is likely to not function");
        }
    }
}
