using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform pointA;
    public Transform pointB;

    [Header("Movement Settings")]
    public float speedMultiplier = 0.5f;
    public float waitTime = 1;

    private bool waiting = false;
    private float timeWaiting = 0;
    private bool movingTowardB = true;
    private float percentMoved = 0;

    // Start is called before the first frame update
    void Start()
    {
      transform.position = pointA.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
      if (waiting==true)
      {
        Wait();
      } else
      {
        Move();
      }
    }

    private void Wait()
    {
      timeWaiting += Time.deltaTime;
      if (timeWaiting >= waitTime)
      {
        timeWaiting = 0;
        waiting = false;
      }
    }

    private void Move()
    {
      if (movingTowardB)
      {
        percentMoved += Time.deltaTime * speedMultiplier;
        if (percentMoved >= 1)
        {
          Debug.Log("Platform percentMoved=" + percentMoved + " at Waypoint B. Wait and then move toward Waypoint A.");
          waiting = true;
          movingTowardB = false;
        }
      } else
      {
        percentMoved -= Time.deltaTime * speedMultiplier;
        if (percentMoved <= 0)
        {
        Debug.Log("Platform percentMoved=" + percentMoved + " at Waypoint A. Wait and then move toward Waypoint B.");
        waiting = true;
        movingTowardB = true;
        }
      }
      transform.position = Vector3.Lerp(pointA.position, pointB.position, percentMoved);
    }
}