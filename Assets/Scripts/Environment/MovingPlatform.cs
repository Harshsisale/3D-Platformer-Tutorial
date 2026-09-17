using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform pointA;
    public Transform pointB;

    [Header("Movement Settings")]
    public float speed = 0.5f;
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
    void Update()
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
        percentMoved += Time.deltaTime * speed;
        if (percentMoved >= 1)
        {
          waiting = true;
          movingTowardB = false;
        }
      } else
      {
        percentMoved -= Time.deltaTime * speed;
        if (percentMoved <= 0)
        {
        waiting = true;
        movingTowardB = true;
        }
      }
      transform.position = Vector3.Lerp(pointA.position, pointB.position, percentMoved);
    }
}