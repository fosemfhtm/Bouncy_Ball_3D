using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveBlock : MonoBehaviour
{
    public double dir = 1f;
    public double speed;
    private double prevTime;
    // Start is called before the first frame update
    void Start()
    {
        speed = 8;
        prevTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, 0, (float)(speed * dir * (Time.time - prevTime))));
        prevTime = Time.time;
    }
    
    void OnTriggerEnter(Collider other) {
        if(other.tag == "blocking") {
            dir *= -1;
        }
    }
}