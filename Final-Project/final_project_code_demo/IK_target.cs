// student ID: 301586823
// student name: Gouttham Nambirajan
// email: gna23@sfu.ca

// Student ID: 301586596
// Student Name: Wenhe Wang
// Student Email: wwa118@sfu.ca

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class target : MonoBehaviour
{
    public Transform body;                  //body
    public LayerMask terrainLayer;          //detection layer
    Vector3 newposition, oldposition, currentposition; //position

    public float footSpacing1,footSpacing2; //offset
    public float stepstance;                //step distance
    public float height = 0.1f;               //height
    public float speed = 2;                 //speed
    float lerp = 1;

    public target leg1, leg2;                //constraints

    private void Start()
    {
        newposition = transform.position;
        currentposition = transform.position;
    }

    void Update()
    {
        transform.position = currentposition;
        //Debug.Log(body.position);

        //angleX: Rotate around the X-axis (tilting up or down).
        //angleY: Rotate around the Y-axis (rotating left or right).
        //angleZ: Rotate around the Z-axis (rolling the ray).
        Vector3 adjustedDirection = Quaternion.Euler(0f, 200f, -75f) * -body.up;
        Ray ray = new Ray(body.position + (body.up * footSpacing1)+(body.right*footSpacing2), adjustedDirection);

        Debug.DrawRay(ray.origin, ray.direction * 10, Color.blue);

        if (Physics.Raycast(ray,out RaycastHit info, 10, terrainLayer.value))
        {
            if (Vector3.Distance(newposition, info.point) > stepstance && leg1.lerp >= 1 && leg2.lerp >= 1)
            {
                lerp = 0;
                newposition = info.point;
            }
        }

        if (lerp < 1)
        {
            // Calculate foot trajectory with height arc
            float stepProgress = lerp; // Progress between 0 and 1
            Vector3 footposition = Vector3.Lerp(oldposition, newposition, stepProgress);
            footposition.y += Mathf.Sin(stepProgress * Mathf.PI) * height; // Arc-like motion
            currentposition = footposition;

            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldposition = newposition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newposition, 0.2f);
    }
}
