using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLikeChild : MonoBehaviour
{
    [Header("Parent Settings")]
    [Tooltip("The Parent transform to follow as if the attached gameobejct was a child of it")]
    public Transform parent;

    [Header("Following Control")]
    [Tooltip("Whether or not this script forces the follow in its own update, turn this off if you want to call this script's functions through another script to maintain call order")]
    [InspectorName("Follow Through This Script's Update")]
    public bool followThroughThisScriptsUpdate = true;

    [Header("Position Following Settings")]
    [Tooltip("Whether or not to follow along the X position")]
    public bool followXPosition = true;
    [Tooltip("Whether or not to follow along the Y position")]
    public bool followYPosition = true;
    [Tooltip("Whether or not to follow along the Z position")]
    public bool followZPosition = true;

    [Header("Rotation Following Settings")]
    [Tooltip("Whether or not to follow along the X rotation")]
    public bool followXRotation = true;
    [Tooltip("Whether or not to follow along the Y rotation")]
    public bool followYRotation = true;
    [Tooltip("Whether or not to follow along the Z rotation")]
    public bool followZRotation = true;

    public enum Orientation { Local, Global };

    [Header("Offset Settings")]
    [Tooltip("If set to true the position of the game object when the game starts is used as and overrides the position offset")]
    public bool useStartingPositionAsOffset = false;
    [Tooltip("Which orientation to use when getting the position offset at the start, only matters if Use Starting Position as Offset is checked")]
    public Orientation positionOffsetOrientation = Orientation.Local;
    [Tooltip("If set to true the rotation of the game object when the game starts is used as and overrides the rotation offset")]
    public bool useStartingRotationAsOffset = false;
    [Tooltip("Which orientation to use when getting the rotation offset at the start, only matters if Use Rotation as Offset is checked")]
    public Orientation rotationOffsetOrientation = Orientation.Local;
    [Tooltip("The positional offset to follow at")]
    public Vector3 positionOffset;
    [Tooltip("The rotation offset in eular angles")]
    public Vector3 eularAngleRotationOffset;

    // Start is called before the first frame update
    void Start()
    {
        DoInitialization();
    }
    
    /// <summary>
    /// Handles the initial settings of this script's variables if needed
    /// </summary>
    void DoInitialization()
    {
        if (useStartingRotationAsOffset)
        {
            if (rotationOffsetOrientation == Orientation.Local)
            {
                eularAngleRotationOffset = transform.localRotation.eulerAngles;
            }
            else if (rotationOffsetOrientation == Orientation.Global)
            {
                eularAngleRotationOffset = transform.rotation.eulerAngles;
            }         
        }
        
        if (useStartingPositionAsOffset)
        {
            if (positionOffsetOrientation == Orientation.Local)
            {
                positionOffset = transform.localPosition;
            }
            else if (positionOffsetOrientation == Orientation.Global)
            {
                positionOffset = transform.position;
            }
        }

        if (followThroughThisScriptsUpdate)
        {
            FollowParent();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (followThroughThisScriptsUpdate)
        {
            FollowParent();
        }
    }

    /// <summary>
    /// Makes the attached game object follow the assigned parent in accorance with this script's settings
    /// </summary>
    public void FollowParent()
    {
        if (followXPosition || followYPosition || followZPosition)
        {
            FollowPosition();
        }
        if (followXRotation || followYRotation || followZRotation)
        {
            FollowRotation();
        }
    }

    /// <summary>
    /// Makes the attached game object's transform follow the parent's position in accordance with the settings
    /// </summary>
    void FollowPosition()
    {
        Vector3 newPosition = new Vector3();
        newPosition.x = followXPosition ? parent.position.x + positionOffset.x : transform.position.x;
        newPosition.y = followYPosition ? parent.position.y + positionOffset.y : transform.position.y;
        newPosition.z = followZPosition ? parent.position.z + positionOffset.z : transform.position.z;
        transform.position = newPosition;
    }

    /// <summary>
    /// Makes the attached game object's transform follow the parent's rotation in accordance with the settings
    /// </summary>
    void FollowRotation()
    {
        Vector3 newRotationInEular = new Vector3();
        newRotationInEular.x = followXRotation ? parent.rotation.eulerAngles.x + eularAngleRotationOffset.x : transform.rotation.eulerAngles.x;
        newRotationInEular.x = followYRotation ? parent.rotation.eulerAngles.y + eularAngleRotationOffset.y : transform.rotation.eulerAngles.y;
        newRotationInEular.x = followZRotation ? parent.rotation.eulerAngles.z + eularAngleRotationOffset.z : transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(parent.rotation.eulerAngles + eularAngleRotationOffset);
    }
}
