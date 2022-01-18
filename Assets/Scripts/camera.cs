using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
    public GameObject player;
    public PlayerBall player_script;
    public GameObject controlY;
    public cameraY controlY_script;
    
    public float rotateSpeed = 2f;
    public float wheelSpeed = 10f;
    private float distance = 30f;
    private float scroll_higher_bound = 100f;
    private float scroll_lower_bound = 10f;
    public bool Third_View = true;

    private Vector3 Camera_Height;
    private Vector3 Third_View_Side;
     
    private float dx = 0;
    private float dy = 0;
    private float damp_velocity = 15f, smoothTime=0.5f, newY;

    public Vector3 pure_CamView;
    public bool scroll_enable = true;


    void Start()
    {
        Camera_Height = new Vector3(0, 5f, 0f);
        Third_View_Side = new Vector3(-1f, 0f, 0f);
        player_script = (PlayerBall) player.GetComponent(typeof(PlayerBall));
        controlY_script = (cameraY) controlY.GetComponent(typeof(cameraY));
        scroll_enable = true;
    }

    // Update is called once per frame
    void Update()
    {
        pure_CamView = transform.position + transform.rotation * (new Vector3(0.0f, 0.0f, 10f));
        if(controlY_script.ball_following) newY = Mathf.SmoothDamp(newY, controlY.transform.position.y, ref damp_velocity, smoothTime/2f);
        else newY = Mathf.SmoothDamp(newY, controlY.transform.position.y, ref damp_velocity, smoothTime);

        if (Input.GetMouseButton(1))
        {
            dx += Input.GetAxis("Mouse X");
            dy -= Input.GetAxis("Mouse Y");
        }
        
        transform.rotation = Quaternion.Euler(dy * rotateSpeed, dx * rotateSpeed, 0);

        if (Input.GetMouseButtonDown(2)) {
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
            if(scroll_enable) {
                distance -= Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
                if (distance < scroll_lower_bound) distance = scroll_lower_bound;
                if (distance > scroll_higher_bound) distance = scroll_higher_bound;
            }

            Camera_View += transform.rotation * Third_View_Side;
            Camera_View.y = newY;

            Vector3 reverseDistance = new Vector3(0.0f, 0.0f, distance);
            transform.position = Camera_View - transform.rotation * reverseDistance;
        }
    }
}