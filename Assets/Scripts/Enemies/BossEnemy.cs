using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class which derives from the Enemy base class. Handles ground enemy movement
/// </summary>
public class BossEnemy : Enemy
{
    [Header("Boss Enemy Settings")]
    [Tooltip("The nav mesh agent used to move this enemy")]
    public NavMeshAgent agent = null;
    [Tooltip("The distance at which to stop before reaching the objective")]
    public float stopDistance = 2.0f;
    [Tooltip("Whether this enemy can stop if it is within it's stop distance but does not have line of sight to it's target")]
    public bool lineOfSightToStop = true;
    [Tooltip("Whether this enemy should always face the player, or face in the direction it is moving")]
    public bool alwaysFacePlayer = true;

    [SerializeField]
    private int stage = 0;

    [Tooltip("How many health points remaining to move to Stage 1 - move toward player")]
    public int stage1Health = 10;
    [Tooltip("Speed in Stage 1")]
    public float stage1Speed = 2.0f;

    [Tooltip("How many health points remaining to move to Stage 2 - spin pattern")]
    public int stage2Health = 5;
    [Tooltip("Speed in Stage 2")]
    public float stage2Speed = 10.0f;
    [Tooltip("Location of points to move to in stage 2")]
    public Transform[] Stage2MovePoints;

    [Tooltip("How many health points remaining to move to Stage 3 - flee")]
    public int stage3Health = 2;
    [Tooltip("Speed in Stage 3")]
    public float stage3Speed = 1.0f;
    [Tooltip("Location of points to move to in stage 3")]
    public Transform[] Stage3FleePoints;

    [Tooltip("Boss Animator")]
    public Animator bossAnimator;

    [Tooltip("Effect to play on Boss stage transition")]
    public GameObject bossTransitionEffect;

    // referemce to bossHealth
    private BossHealth bossHealth;



    /// <summary>
    /// Description:
    /// Overrides base function setup, sets up nav mesh agent reference
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    protected override void Setup()
    {
        base.Setup();
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (agent != null)
        {
            agent.updateRotation = !alwaysFacePlayer;
        }

        // get the bossHealth;
        bossHealth = GetComponent<BossHealth>();
    }


    /// <summary>
    /// Description:
    /// Handles movement for this enemy
    /// Rotates the enemy to face it's target and sets navmesh destination
    /// Inputs:
    /// none
    /// Outputs:
    /// void
    /// </summary>
    protected override void HandleMovement()
    {
        // determine if should move to different stage of attack based on bossHealth
        if (bossHealth.currentHealth <= stage3Health) // stage3
        {
            if (stage != 3) // transition to stage
            {
                if (bossAnimator != null)
                {
                    bossAnimator.SetTrigger("Injured");
                }

                if (bossTransitionEffect != null)
                {
                    Instantiate(bossTransitionEffect, transform.position, transform.rotation, null);
                }
            }
            stage = 3;
            canMove = true;
            moveSpeed = stage3Speed;
            alwaysFacePlayer = false;

        }
        else if (bossHealth.currentHealth <= stage2Health) // stage2
        {
            if (stage != 2)
            {
                if (bossAnimator != null)
                {
                    bossAnimator.SetTrigger("Spin");
                }

                if (bossTransitionEffect != null)
                {
                    Instantiate(bossTransitionEffect, transform.position, transform.rotation, null);
                }
            }

            stage = 2;
            canMove = true;
            moveSpeed = stage2Speed;
            alwaysFacePlayer = false;

        }
        else if (bossHealth.currentHealth <= stage1Health) // stage1
        {
            if (stage != 1)
            {
                if (bossAnimator != null)
                {
                    bossAnimator.SetTrigger("Bounce");
                }

                if (bossTransitionEffect != null)
                {
                    Instantiate(bossTransitionEffect, transform.position, transform.rotation, null);
                }
            }
            stage = 1;
            canMove = true;
            moveSpeed = stage1Speed;
            alwaysFacePlayer = true;
        }


        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }

        Quaternion desiredRotation = CalculateDesiredRotation();
        transform.rotation = desiredRotation;

        /// <summary>
        /// Description:
        /// Small function which determines whether or not this enemy should move
        /// Inputs: N/A
        /// Outputs: bool
        /// </summary>
        /// <returns>bool: whether or not this enemy should move</returns>
        bool ShouldMove()
        {
            if (agent != null && canMove)
            {
                if (stage == 1 && target != null) // only move to player in stage 1
                {
                    if ((target - transform.position).magnitude > stopDistance)
                    {
                        return true;
                    }
                    else if (awareness != null && lineOfSightToStop && !awareness.CheckLineOfSight())
                    {
                        return true;
                    }
                }
                else if (stage > 1)
                {
                    return true;
                }

            }
            return false;
        }

        if (travelingToSpecificTarget)
        {
            if (Time.time >= timeToStopTrying || NavMeshAgentDestinationReached())
            {
                travelingToSpecificTarget = false;
                //agent.SetDestination(target);
            }
        }
        else if (ShouldMove())
        {
            switch (stage)
            {
                case 3: // move to a random Stage3FleePoint
                    if (Stage3FleePoints.Length > 0)
                    {
                        GoToTarget(Stage3FleePoints[Random.Range(0, Stage3FleePoints.Length - 1)].position, 10f);
                    }
                    break;
                case 2: // move to a random Stage2MovePoints
                    if (Stage2MovePoints.Length > 0)
                    {
                        GoToTarget(Stage2MovePoints[Random.Range(0, Stage2MovePoints.Length - 1)].position, 10f);
                    }
                    break;
                case 1:
                    agent.SetDestination(target);
                    break;
            }

        }
        else if (agent != null)
        {
            agent.SetDestination(transform.position);
        }
    }

    /// <summary>
    /// Description:
    /// Checks to see if the nav mesh agent has reached its destination or not
    /// Input:
    /// none
    /// Returns:
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not the agent has reached its destination</returns>
    bool NavMeshAgentDestinationReached()
    {
        // Check if we've reached the destination
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Calculates the movement that this enemy is/will make this frame.
    /// Inputs: N/A
    /// Outputs: Vector3
    /// </summary>
    /// <returns>The desired movement of this enemy</returns>
    protected override Vector3 CalculateDesiredMovement()
    {
        if (agent != null)
        {
            return agent.desiredVelocity * Time.deltaTime;
        }
        return base.CalculateDesiredMovement();
    }

    /// <summary>
    /// Description:
    /// Caclulates the desired rotation of this enemy this frame
    /// Inputs: N/A
    /// Outputs: Quaternion
    /// </summary>
    /// <returns>The desired rotation of the enemy</returns>
    protected override Quaternion CalculateDesiredRotation()
    {
        if (alwaysFacePlayer)
        {
            if (awareness != null && awareness.certaintyOfPlayer > awareness.followThreshold)
            {
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0;
                Quaternion result = Quaternion.LookRotation(toTarget, transform.up);
                return result;
            }
            else
            {
                return transform.rotation;
            }
        }
        return base.CalculateDesiredRotation();
    }

    bool travelingToSpecificTarget = false;
    float timeToStopTrying = 0;

    /// <summary>
    /// Description:
    /// Makes the enemy nav agent go to a specific location and will continue to try to reach that location until
    /// the specified time has passed
    /// Input:
    /// Vector3 target, float timeToSpend
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="target">The target to travel to</param>
    /// <param name="timeToSpend">The time to spend trying to get to that target</param>
    public void GoToTarget(Vector3 target, float timeToSpend)
    {
        agent.SetDestination(target);
        travelingToSpecificTarget = true;
        timeToStopTrying = Time.time + timeToSpend;
    }
}
