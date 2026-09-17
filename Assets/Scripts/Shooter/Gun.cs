using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the setup of a gun
/// </summary>
public class Gun : MonoBehaviour
{
    [Header("Prefab Settings")]
    [Tooltip("The projectile game object to instantiate when firing this gun")]
    public GameObject projectileGameObject;
    [Tooltip("Whether or not the fired projectile should be a child of the fire location")]
    public bool childProjectileToFireLocation = false;
    [Tooltip("The effect prefab to instantiate when firing this gun")]
    public GameObject fireEffect;

    [Header("Fire settings")]
    [Tooltip("The transform whos location this fires from")]
    public Transform fireLocationTransform;
    [Tooltip("How long to wait before being able to fire again, if no animator is set")]
    public float fireDelay = 0.02f;
    [Tooltip("The fire type of the weapon")]
    public FireType fireType = FireType.semiAutomatic;

    // enum for setting the fire type
    public enum FireType { semiAutomatic, automatic };

    // The time when this gun will be able to fire again
    private float ableToFireAgainTime = 0;

    [Tooltip("The number of projectiles to fire when firing")]
    public int maximumToFire = 1;
    [Tooltip("The maximum degree (eular angle) of spread shots can be fired in")]
    [Range(0, 45)]
    public float maximumSpreadDegree = 0;

    [Header("Equipping settings")]
    [Tooltip("Whether or not this gun is available for use")]
    public bool available = false;

    [Header("Animation Settings")]
    [Tooltip("The animator that animates this gun.")]
    public Animator gunAnimator = null;
    [Tooltip("Shoot animator trigger name")]
    public string shootTriggerName = "Shoot";
    [Tooltip("The animation state anme when the gun is idle (used to handle when we are able to fire again)")]
    public string idleAnimationName = "Idle";

    /// <summary>
    /// Description:
    /// Fires the gun, creating both the projectile and fire effect
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void Fire()
    {
        bool canFire = false;

        // use the animator for fire delay if possible
        // otherwise use the timing set in the inspector
        if (gunAnimator != null)
        {
            canFire = gunAnimator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimationName);
        }
        else
        {
            canFire = ableToFireAgainTime <= Time.time;
        }

        if (canFire)
        {
            if (projectileGameObject != null)
            {
                for (int i = 0; i < maximumToFire; i++)
                {
                    float fireDegreeX = Random.Range(-maximumSpreadDegree, maximumSpreadDegree);
                    float fireDegreeY = Random.Range(-maximumSpreadDegree, maximumSpreadDegree);
                    Vector3 fireRotationInEular = fireLocationTransform.rotation.eulerAngles + new Vector3(fireDegreeX, fireDegreeY, 0);
                    GameObject projectile = Instantiate(projectileGameObject, fireLocationTransform.position,
                        Quaternion.Euler(fireRotationInEular), null);
                    if (childProjectileToFireLocation)
                    {
                        projectile.transform.SetParent(fireLocationTransform);
                    }
                }
            }

            if (fireEffect != null)
            {
                Instantiate(fireEffect, fireLocationTransform.position, fireLocationTransform.rotation, fireLocationTransform);
            }

            ableToFireAgainTime = Time.time + fireDelay;
            PlayShootAnimation();
        }
    }

    /// <summary>
    /// Description:
    /// Tries to play a shoot animation on the gun
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void PlayShootAnimation()
    {
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger(shootTriggerName);
        }
    }
}
