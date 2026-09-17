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
   
    }
    // Update is called once per frame
    void Update()
    {
   
    }
}