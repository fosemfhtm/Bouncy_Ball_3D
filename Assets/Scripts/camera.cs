using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public GameObject player;
    public float rotateSpeed = 2f;
    public float wheelSpeed = 10f;
    public float distance = 5f;
    public float scroll_higher_bound = 30f;
    public float scroll_lower_bound = 1f;
    public bool Third_View = true;

    private Vector3 Camera_Height;
    private Vector3 Third_View_Side;
    
    private float dx = 0;
    private float dy = 0;


    void Start()
    {
        Camera_Height = new Vector3(0, 5f, 0f);
        Third_View_Side = new Vector3(-1f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            dx += Input.GetAxis("Mouse X");
            dy -= Input.GetAxis("Mouse Y");
        }
        
        transform.rotation = Quaternion.Euler(dy * rotateSpeed, dx * rotateSpeed, 0);

        if (Input.GetMouseButtonDown(0)) {
            Third_View = !Third_View;
        }

        Vector3 Camera_View = player.transform.position;

        if (!Third_View)
        {
            Camera_View += Camera_Height * 0.6f;
            Vector3 reverseDistance = new Vector3(0.0f, 0.0f, 0.5f);
            transform.position = Camera_View + transform.rotation * reverseDistance;
        }
        else {
            distance -= Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
            if (distance < scroll_lower_bound) distance = scroll_lower_bound;
            if (distance > scroll_higher_bound) distance = scroll_higher_bound ;

            Camera_View += transform.rotation * Third_View_Side;
            Camera_View.y = Camera_Height.y;
            Vector3 reverseDistance = new Vector3(0.0f, 0.0f, distance);
            transform.position = Camera_View - transform.rotation * reverseDistance;           
        }
    }
}