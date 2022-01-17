using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main_ball : MonoBehaviour
{
    Rigidbody rigid;
    
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        rigid.velocity = Vector3.up * 20f;
    }
        
}
