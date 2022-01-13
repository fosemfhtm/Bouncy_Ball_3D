using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public float jumpPower = 10;
    public float factor = 0.7f;
    public float max_velocity = 10f;
    public float friction_force = 0.5f;
    public float velocity_lowerbound = 0.26f;
    Rigidbody rigid;
    void Awake() {
        rigid = GetComponent<Rigidbody>();
    }

    void Update() {
        if(Input.GetButtonDown("Jump")) {
            rigid.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
        }
    }
    
    void FixedUpdate() { 
        Debug.Log(rigid.velocity.z);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool stop = false, keep_max = false, change_dir_x = false, change_dir_z = false;
        float stop_x=rigid.velocity.x, stop_z=rigid.velocity.z;
        float max_x=rigid.velocity.x, max_z=rigid.velocity.z;

        if(Mathf.Abs(rigid.velocity.x) >= velocity_lowerbound && h*rigid.velocity.x<0f) {
            change_dir_x = true;
        }

        if(Mathf.Abs(rigid.velocity.z) >= velocity_lowerbound && v*rigid.velocity.z<0f) {
            change_dir_z = true;
        }

        if(Mathf.Abs(rigid.velocity.x) <= max_velocity) {
            float force_control;

            if(change_dir_x) {
                force_control = 2.5f*h*factor;
            }
            else {
                force_control = h*factor;
            }

            if(Mathf.Abs(rigid.velocity.x + force_control) > max_velocity) {
                force_control = h*(max_velocity - Mathf.Abs(rigid.velocity.x));
            }

            rigid.AddForce(new Vector3(force_control, 0, 0), ForceMode.Impulse);
            // if(change_dir_x) rigid.AddForce(new Vector3(2.5f*h*factor, 0, 0), ForceMode.Impulse);
            // else rigid.AddForce(new Vector3(h*factor, 0, 0), ForceMode.Impulse);
        }
        
        if(Mathf.Abs(rigid.velocity.z) <= max_velocity) {
            float force_control;

            if(change_dir_z) {
                force_control = 2.5f*v*factor;
            }
            else {
                force_control = v*factor;
            }

            if(Mathf.Abs(rigid.velocity.z + force_control) > max_velocity) {
                force_control = v*(max_velocity - Mathf.Abs(rigid.velocity.z));
            }

            rigid.AddForce(new Vector3(0, 0, force_control), ForceMode.Impulse);
            // if(change_dir_z) rigid.AddForce(new Vector3(0, 0, 2.5f*v*factor), ForceMode.Impulse);
            // else rigid.AddForce(new Vector3(0, 0, v*factor), ForceMode.Impulse);
        }

        if(h==0) {
            if(Mathf.Abs(rigid.velocity.x) >= velocity_lowerbound) {
                if(rigid.velocity.x > 0f) {
                    rigid.AddForce(new Vector3(-friction_force, 0, 0), ForceMode.Impulse);    
                }
                else {
                    rigid.AddForce(new Vector3(friction_force, 0, 0), ForceMode.Impulse);
                }
            }
            else {
                stop = true;
                stop_x = 0f;
            }
        }
        else if(Mathf.Abs(rigid.velocity.x) > max_velocity) {
            keep_max = true;
            if(rigid.velocity.x > 0f) {
                max_x = max_velocity;
            }
            else {
                max_x = -max_velocity;
            }
        }

        if(v==0) {
            if(Mathf.Abs(rigid.velocity.z) >= velocity_lowerbound) {
                if(rigid.velocity.z > 0f) {
                    rigid.AddForce(new Vector3(0, 0, -friction_force), ForceMode.Impulse);
                }
                else {
                    rigid.AddForce(new Vector3(0, 0, friction_force), ForceMode.Impulse);
                }
            }
            else {
                stop = true;
                stop_z = 0f;
            }
        }
        else if(Mathf.Abs(rigid.velocity.z) > max_velocity){
            keep_max = true;
            if(rigid.velocity.z > 0f) {
                max_z = max_velocity;
            }
            else {
                max_z = -max_velocity;
            }
        }

        if(stop) {
            rigid.velocity = new Vector3(stop_x, rigid.velocity.y, stop_z);
        }
        if(keep_max) rigid.velocity = new Vector3(max_x, rigid.velocity.y, max_z);

        // if(h==0 && v==0) rigid.velocity = new Vector3(rigid.velocity.x*0.9f, rigid.velocity.y, rigid.velocity.z*0.9f);
        // if(h==0 && v!=0) rigid.velocity = new Vector3(rigid.velocity.x*0.9f, rigid.velocity.y, rigid.velocity.z);
        // if(h!=0 && v==0) rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y, rigid.velocity.z*0.9f);
    }

    void OnCollisionEnter(Collision collision)
    {
        ExcecuteReBounding(collision);
    }
    
    
     void ExcecuteReBounding(Collision collision)
    {
        ContactPoint cp = collision.GetContact(0);
        Vector3 dir = rigid.position - cp.point;
        GetComponent<Rigidbody>().AddForce((dir).normalized * 1200f);
    }


//     void FixedUpdate()
//    {
       
//        Debug.Log(CrossPlatformInputManager.GetAxis("Horizontal"));
//        directionX = CrossPlatformInputManager.GetAxis("Horizontal");
//        directionY = CrossPlatformInput

//       if (Mathf.Abs (directionX) > 0.05f) {
//               PlayerBody.velocity = new Vector3(directionX * MovmentSpeed, 0);
//       } else {
//               PlayerBody.velocity = Vector3.zero;
//       }
//    }
}
