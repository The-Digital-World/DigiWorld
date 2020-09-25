using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFollow : MonoBehaviour
{
    public float speed;
    private Transform target;
    public float stopDistance;


    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        stopDistance = 3;
    }

    // Update is called once per frame
    void Update()
    {


        if(Vector2.Distance(transform.position, target.position) > stopDistance)
        {
           transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
        }
        



    }
}
