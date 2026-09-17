using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

/// <summary>
/// This class handles the movement and orientation of the third person camera
/// </summary>
public class ThirdPersonCamera : MonoBehaviour 
{
    [Header("Needed References")]
    [Tooltip("The player character controller")]
    public CharacterController player = null;
    [Tooltip("This is what the camera looks at, can be the player or any another gameobject")]
    public GameObject lookAt;

    [Header("Input Actions & Controls")]
    [Tooltip("The input action(s) that map to where the controller looks")]
    public InputAction lookInput;

    // the rotation reference rotated by this script to get the camera functionality desired
    private Transform rotationReference;

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    private void OnEnable()
    {
        lookInput.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    private void OnDisable()
    {
        lookInput.Disable();
    }

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
    public void InitialSetup()
    {
        // Getting control settings from the player prefs if the exist
        if (PlayerPrefs.HasKey("InvertX"))
        {
            if (PlayerPrefs.GetInt("InvertX") != 0)
            {
                SetInvertedHorizontal(true);
            }
            else if (PlayerPrefs.GetInt("InvertX") == 0)
            {
                SetInvertedHorizontal(false);
            }
        }
        if (PlayerPrefs.HasKey("InvertY"))
        {
            if (PlayerPrefs.GetInt("InvertY") != 0)
            {
                SetInvertedVertical(true);
            }
            else if (PlayerPrefs.GetInt("InvertY") == 0)
            {
                SetInvertedVertical(false);
            }
        }
        if (PlayerPrefs.HasKey("SensitivitySlider"))
        {
            cameraHorizontalSpeed = PlayerPrefs.GetFloat("SensitivitySlider");
            cameraVerticalSpeed = PlayerPrefs.GetFloat("SensitivitySlider");
        }

        if (player == null)
        {
            Debug.LogError("There is no character controller reference set for the third person camera. + " +
                "\n It needs one to run correctly");
        }

        rotationReference = transform.parent;
        if (rotationReference == null)
        {
            Debug.LogError("The third person camera is not a child of a gameobject with a CameraRotationHelper script attached \n" +
                "It needs to be in order to function correctly");
        }
        else if (rotationReference.GetComponent<CameraRotationHelper>() == null)
        {
            Debug.LogError("The third person camera is not a child of a gameobject with a CameraRotationHelper script attached \n" +
                "It needs to be in order to function correctly");
        }
    }

    // Wait this many frames before starting to process the camera rotation
    int waitForFrames = 3;
    int framesWaited = 0;

    /// <summary>
    /// Description:
    /// Standard unity function called once per frame after update
    /// Input:
    /// none
    /// Return:
    /// void
    /// </summary>
    void LateUpdate() 
    {
        // Wait so many frames to avoid startup camera movement bug
        if (framesWaited <= waitForFrames)
        {
            framesWaited += 1;
            return;
        }

        // Do nothing if paused
        if (Time.timeScale == 0)
        {
            return;
        }

        FaceCharacter();
        CentralizedControl(lookInput.ReadValue<Vector2>().x, lookInput.ReadValue<Vector2>().y);
    }

    /// <summary>
    /// Description:
    /// Makes the camera face its look at target which is typically set to the player character or a child of the
    /// player at an offset
    /// Input:
    /// none
    /// Return:
    /// void
    /// </summary>
    void FaceCharacter()
    {
        this.transform.LookAt(lookAt.transform);
    }

    [Header("Speed Controls")]
    [Tooltip("How quickly the camera moves horizontally")]
    public float cameraHorizontalSpeed = 180f;
    [Tooltip("How quickly the camera moves vertically")]
    public float cameraVerticalSpeed = 180f;
    [Tooltip("The limit on how quickly the camera can move backward when there is space for it to do so")]
    [Range(1, 10)]
    public float maximumBackWardMovespeed = 10f;

    [Header("Inversion Control Settings")]
    [Tooltip("Whether or not to invert the horizontal look")]
    public bool invertedHorizontal = false;
    [Tooltip("Whether or not to invert the vertical look")]
    public bool invertedVertical = false;

    [Header("Camera Vertical Rotation Caps (in degrees)")]
    [Tooltip("The maximum X rotation to rotate to in degrees")]
    [Range(70, 85)]
    public float maximumXRotation = 85f;
    [Tooltip("The minimum X rotation to rotate to in degrees")]
    [Range(10, -85)]
    public float minimumXRotation = 0f;

    // Takes the player's input and rotates a camera accordingly
    void CentralizedControl(float horizontalLook, float verticalLook)
    {
        float horizontalSensitivityMultiplier = 1;
        float verticalSensitivityMultiplier = 1;

        if (PlayerPrefs.HasKey("HorizontalMouseSensitivity"))
        {
            horizontalSensitivityMultiplier = PlayerPrefs.GetFloat("HorizontalMouseSensitivity");
        }

        if (PlayerPrefs.HasKey("VerticalMouseSensitivity"))
        {
            verticalSensitivityMultiplier = PlayerPrefs.GetFloat("VerticalMouseSensitivity");
        }

        // Camera input
        float horizontalInput = horizontalLook * horizontalSensitivityMultiplier;
        float verticalInput = verticalLook * verticalSensitivityMultiplier;

        // Camera movement inversion
        if (invertedHorizontal)
        {
            horizontalInput = -horizontalInput;
        }
        if (invertedVertical)
        {
            verticalInput = -verticalInput;
        }

        // How much to adjust the rotation horizontally
        float horizontalRotationAdjustment = horizontalInput * cameraHorizontalSpeed * Time.deltaTime;
        // How much to adjust the rotation vertically
        float verticalRotationAdjustment = verticalInput * cameraVerticalSpeed * Time.deltaTime;

        // The eular angles of the rotation we are changing to
        Vector3 rotationToChangeToInEular = rotationReference.rotation.eulerAngles;
        Vector3 currentRotationToChangeToInEular = rotationReference.rotation.eulerAngles;

        // Apply the horizontal rotation if it is non-zero
        if (horizontalRotationAdjustment != 0)
        {
            // This line applies just the horizontal rotation adjustment by using the eular degrees, taking the same x and z of the 
            // rotation refrence while taking the y and adding the adjustment to it.
            rotationToChangeToInEular = new Vector3(rotationToChangeToInEular.x, rotationToChangeToInEular.y + horizontalRotationAdjustment, rotationToChangeToInEular.z);
            // This results in chaning only the y value of the rotation reference which then "moves" the camera because it is a child of the
            // rotation reference
        }

        // Apply the vertical rotation if it is non-zero
        if (verticalRotationAdjustment != 0)
        {
            // This line applies just the vertical rotation adjustment by using the eular degrees, taking the same y and z of the 
            // rotation refrence while taking the x and adding the adjustment to it.
            rotationToChangeToInEular = new Vector3(rotationToChangeToInEular.x + verticalRotationAdjustment, rotationToChangeToInEular.y, rotationToChangeToInEular.z);
            // This results in chaning only the x value of the rotation reference which then "moves" the camera because it is a child of the
            // rotation reference
        }

        // The following two if statements handle clamping the X rotation to the maximum and minumum settings set in the editor
        // These stops the camera from going all the way around when rotating vertically
        if (rotationToChangeToInEular.x > maximumXRotation && rotationToChangeToInEular.x < 180)
        {
            rotationToChangeToInEular = new Vector3(maximumXRotation, rotationToChangeToInEular.y, rotationToChangeToInEular.z);
        }
        if (rotationToChangeToInEular.x < minimumXRotation + 360 && rotationToChangeToInEular.x >= 180)
        {
            rotationToChangeToInEular = new Vector3(minimumXRotation, rotationToChangeToInEular.y, rotationToChangeToInEular.z);
        }

        // clamp the rotation 360 - 270 is up 0 - 90 is down
        // Because of the way eular angles work with Unity's rotations we have to act differently when clamping the rotation
        if (rotationToChangeToInEular.x < 270 && rotationToChangeToInEular.x >= 180)
        {
            rotationToChangeToInEular.x = maximumXRotation;
        }
        else if (rotationToChangeToInEular.x > 90 && rotationToChangeToInEular.x < 180)
        {
            rotationToChangeToInEular.x = minimumXRotation;
        }


        //Vector3 newAngle = new Vector3(Mathf.LerpAngle(currentRotationToChangeToInEular.x, rotationToChangeToInEular.x,Time.deltaTime* cameraHorizontalSpeed), Mathf.LerpAngle(currentRotationToChangeToInEular.y, rotationToChangeToInEular.y, Time.deltaTime* cameraVerticalSpeed), Mathf.LerpAngle(currentRotationToChangeToInEular.z, rotationToChangeToInEular.z, Time.deltaTime));
        // Apply the complete rotation adjustment
        //rotationReference.rotation = Quaternion.Euler(rotationToChangeToInEular);
        rotationReference.rotation = Quaternion.Euler(rotationToChangeToInEular);

        // Handle the camera by raycast
        RaycastAdjustments();
    }

    /// <summary>
    /// Description:
    /// Sets the inverted horizontal setting
    /// Input:
    /// bool setTo
    /// Return:
    /// void
    /// </summary>
    /// <param name="setTo">The true or false value to set value to</param>
    public void SetInvertedHorizontal(bool setTo)
    {
        invertedHorizontal = setTo;
    }

    /// <summary>
    /// Description:
    /// Sets the inverted vertical setting
    /// Input:
    /// bool setTo
    /// Return:
    /// void
    /// </summary>
    /// <param name="setTo">The true or false value to set value to</param>
    public void SetInvertedVertical(bool setTo)
    {
        invertedVertical = setTo;
    }

    [Header("Distance Controls")]
    [Tooltip("Maximum distance away from the player")]
    public float maximumDistanceAway = 10f;
    [Tooltip("Maximum distance on the y axis (limits the distance in the y the camera can move away)")]
    public float maximumDistanceOnYAxis = 10f;
    [Tooltip("Minimum distance from player (limits how close the camera can get to the player)")]
    public float minimumDistance = 0.8f;
    [Tooltip("X rotation where the applied maximum distance reaches the minimumDistance")]
    public float xRotationValueWhereMaxDistanceBecomesMinimum = -60f;
    [Tooltip("X Rotation where the applied maximum distance reaches maximum")]
    public float xRotationValueWhereMaxDistanceIsMaximum = 60f;

    [Header("Linear interpolation smoothing")]
    [Tooltip("The amount of smoothing to apply when moving the camera")]
    public float smoothing = 10f;

    [Header("Raycast settings")]
    [Tooltip("The Distance to offset from the collision (Should be slightly less than the players xz scale to avoid moving through the player up close)")]
    public float offsetDistanceFromCollision = 0.4f;

    // The actual maximum distance away the camera can be from the player, used in raycasting
    float appliedMaximumDistanceAway;

    // the additional distance to raycast when checking to see if backward movement of the camera should stop,
    // used to stop the camera before it hits an object
    float additionalIdleDistance = 0.5f;

    /// <summary>
    /// Description:
    /// Uses raycasting to determine adjustments and then makes those adjustments to keep the camera infront of objects in the world
    /// Raycasts back towards the camera
    /// Input:
    /// none
    /// Return:
    /// void
    /// </summary>
    void RaycastAdjustments()
    {
        // The position of the camera, used for raycasting
        Vector3 cameraPostion = this.transform.position;

        // The position of what the camera is looking at, used for raycasting
        Vector3 lookAtPosition = lookAt.transform.position;

        // The direction from the look at gameobject to the camera, effectively pointing from the lookat to the camera
        // used as the direction to raycast in
        Vector3 directionToCameraFromLookAt = (cameraPostion - lookAtPosition).normalized;

        // The current x rotation
        float currentXRotation = rotationReference.rotation.eulerAngles.x;

        // If the current x rotation is greater than 180, convert it to be a negative degree
        if (currentXRotation > 180)
        {
            currentXRotation = currentXRotation - 360;
        }

        // Calculate the ratio of the current X rotation is over the range of values:
        
        // adjust the range to be between 0 and "topOfRange" the sum of the absoulute values of the max and minimum rotation settings
        float topOfRange = Mathf.Abs(xRotationValueWhereMaxDistanceBecomesMinimum) + Mathf.Abs(xRotationValueWhereMaxDistanceIsMaximum);

        // Calculate the ratio of the current X rotation plus the maximum rotation over the ranges maximum
        float ratio = (currentXRotation + xRotationValueWhereMaxDistanceIsMaximum) / topOfRange;

        // Ensure the ratio does not exceed 1
        if (ratio > 1)
        {
            ratio = 1;
        }

        // Calculate the applied maximum distance away the camera can be on this frame using the ratio
        // This value will fluctuate based on the X rotation, if the X rotation is at its max then the applied maximum distance away
        // will be the same as the maximum distance away set in the inspector
        // Doing this allows the camera tomove along a curve when being rotated around the x
        appliedMaximumDistanceAway = (maximumDistanceAway - minimumDistance) * ratio + minimumDistance;

        // Raycast hit information used to tell if the camera needs to move forward
        RaycastHit moveForwardCastHit = new RaycastHit();
        // Move forward cast
        float distanceToCast = (transform.position - lookAt.transform.position).magnitude;
        moveForwardCastHit = RaycastFromLookAtToPoint(transform, distanceToCast);

        // Raycast hit information used to tell if the camera needs to be idle
        RaycastHit idleCastHit = new RaycastHit();
        // Idle (dont move cast)
        distanceToCast = (transform.position - lookAt.transform.position).magnitude + additionalIdleDistance;
        idleCastHit = RaycastFromLookAtToPoint(transform, distanceToCast);

        // If we hit something in the move forward ray cast position the camera accordingly
        if (moveForwardCastHit.transform != null)
        {
            Vector3 lerpTarget = lookAtPosition + (directionToCameraFromLookAt * Mathf.Clamp((moveForwardCastHit.distance - offsetDistanceFromCollision), minimumDistance, appliedMaximumDistanceAway));
            transform.position = Vector3.Lerp(transform.position, lerpTarget, Time.deltaTime * smoothing);
        }
        // If the idle cast hits something, do nothing (idle)
        else if (idleCastHit.transform != null)
        {
            // Do Nothing
        }
        else
        {
            // Raycast from the camera to move the camera in front of objects if needed still, but move backwards towards the maximum otherwise
            RaycastHit centerCameraHit = new RaycastHit();
            Vector3 rayCastDirection = (transform.position - lookAt.transform.position).normalized;
            Physics.Raycast(lookAt.transform.position, rayCastDirection, out centerCameraHit, appliedMaximumDistanceAway);

            if (centerCameraHit.transform != null)
            {
                Vector3 lerpTarget = centerCameraHit.point - (rayCastDirection * offsetDistanceFromCollision);
                transform.position = Vector3.Lerp(transform.position, lerpTarget, Time.deltaTime * smoothing);
            }
            else
            {
                Vector3 lerpTarget = lookAtPosition + (directionToCameraFromLookAt * appliedMaximumDistanceAway);
                transform.position = Vector3.Lerp(transform.position, lerpTarget, Time.deltaTime * smoothing);
            }
        }
    }

    /// <summary>
    /// Description:
    /// Raycasts from the look at gameobject to a target point returns the resulting raycast hit
    /// Input:
    /// Transform point | float distanceToCast
    /// Return:
    /// RaycastHit
    /// </summary>
    /// <param name="point">The point to raycast towards</param>
    /// <param name="distanceToCast">How far to do the ray cast</param>
    /// <returns>The RaycastHit generated by the raycast</returns>
    RaycastHit RaycastFromLookAtToPoint(Transform point, float distanceToCast)
    {
        RaycastHit results;
        Vector3 rayCastDirection = (point.position - lookAt.transform.position).normalized;
        Physics.Raycast(lookAt.transform.position, rayCastDirection, out results, distanceToCast);

        return results;
    }

    RaycastHit RaycastFromGivenTransformToCamera(Transform startingFrom)
    {
        RaycastHit results;
        Vector3 rayCastDirection = (transform.position - startingFrom.position).normalized;

        float distanceToCast = (transform.position - startingFrom.position).magnitude + offsetDistanceFromCollision;
        Physics.Raycast(startingFrom.position, rayCastDirection, out results, distanceToCast);

        return results;
    }
}
