﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{

    public float speed;
    public float waitTime;
    public float startWaitTime;

    public Transform[] moveSpots;
    private int randomSpot;

    private void Start()
    {
        waitTime = startWaitTime; 
        randomSpot = Random.Range(0, moveSpots.Length);

        foreach(Transform spot in moveSpots)
        {
            spot.SetParent(null, true);
        }
    }


    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, moveSpots[randomSpot].position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, moveSpots[randomSpot].position) < 0.2f)
        {
            if(waitTime <= 0)
            {
                randomSpot = Random.Range(0, moveSpots.Length);
                waitTime = startWaitTime;
                
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }
}
