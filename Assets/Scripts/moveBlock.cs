using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveBlock : MonoBehaviour
{
    public float dir = 1f;
    public float speed = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        dir = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, 0, speed * dir));
    }
    
    void OnTriggerEnter(Collider other) {
        if(other.tag == "blocking") {
            dir *= -1;
        }
    }
}