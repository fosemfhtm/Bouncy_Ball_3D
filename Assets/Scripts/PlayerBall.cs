using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public GameObject camera;
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
        Vector3 dir = camera.transform.rotation * Vector3.forward;
        Debug.Log(camera.transform.rotation * Vector3.forward);
        dir.y = 0f;
        Vector3 dir_z = (dir).normalized;
        Vector3 dir_x = -Vector3.Cross(dir_z, new Vector3(0, 1f, 0));

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 velocity_x = proj(rigid.velocity, dir_x);
        Vector3 velocity_z = proj(rigid.velocity, dir_z);

        bool stop = false, keep_max = false, change_dir_x = false, change_dir_z = false;
        Vector3 stop_x = velocity_x, stop_z = velocity_z;
        Vector3 max_x = velocity_x, max_z = velocity_z;

        if(Vector3.Magnitude(velocity_x) >= velocity_lowerbound && h * vecConst(velocity_x, dir_x) < 0f) {
            change_dir_x = true;
        }

        if(Vector3.Magnitude(velocity_z) >= velocity_lowerbound && v * vecConst(velocity_z, dir_z) < 0f) {
            change_dir_z = true;
        }

        if(Vector3.Magnitude(velocity_x) <= max_velocity) {
            float force_control;

            if(change_dir_x) {
                force_control = 2.5f*h*factor;
            }
            else {
                force_control = h*factor;
            }

            if(Vector3.Magnitude(velocity_x + dir_x * force_control) > max_velocity) {
                force_control = h*(max_velocity - Vector3.Magnitude(velocity_x));
            }

            // rigid.AddForce(new Vector3(force_control, 0, 0), ForceMode.Impulse);
            rigid.AddForce(dir_x * force_control, ForceMode.Impulse);
        }
        
        if(Vector3.Magnitude(velocity_z) <= max_velocity) {
            float force_control;

            if(change_dir_z) {
                force_control = 2.5f*v*factor;
            }
            else {
                force_control = v*factor;
            }

            if(Vector3.Magnitude(velocity_z + dir_z * force_control) > max_velocity) {
                force_control = v*(max_velocity - Vector3.Magnitude(velocity_z));
            }

            // rigid.AddForce(new Vector3(0, 0, force_control), ForceMode.Impulse);
            rigid.AddForce(dir_z * force_control, ForceMode.Impulse);
        }

        if(h==0) {
            if(Vector3.Magnitude(velocity_x) >= velocity_lowerbound) {
                if(vecConst(velocity_x, dir_x) > 0f) {
                    // rigid.AddForce(new Vector3(-friction_force, 0, 0), ForceMode.Impulse);
                    rigid.AddForce(-friction_force * dir_x, ForceMode.Impulse);
                }
                else {
                    // rigid.AddForce(new Vector3(friction_force, 0, 0), ForceMode.Impulse);
                    rigid.AddForce(friction_force * dir_x, ForceMode.Impulse);
                }
            }
            else {
                stop = true;
                stop_x = dir_x * 0f;
            }
        }
        else if(Vector3.Magnitude(velocity_x) > max_velocity) {
            keep_max = true;
            if(vecConst(velocity_x, dir_x) > 0f) {
                max_x = dir_x * max_velocity;
            }
            else {
                max_x = -dir_x * max_velocity;
            }
        }

        if(v==0) {
            if(Vector3.Magnitude(velocity_z) >= velocity_lowerbound) {
                if(vecConst(velocity_z, dir_z) > 0f) {
                    // rigid.AddForce(new Vector3(0, 0, -friction_force), ForceMode.Impulse);
                    rigid.AddForce(-friction_force * dir_z, ForceMode.Impulse);
                }
                else {
                    // rigid.AddForce(new Vector3(0, 0, friction_force), ForceMode.Impulse);
                    rigid.AddForce(friction_force * dir_z, ForceMode.Impulse);
                }
            }
            else {
                stop = true;
                stop_z = dir_z * 0f;
            }
        }
        else if(Vector3.Magnitude(velocity_z) > max_velocity){
            keep_max = true;
            if(vecConst(velocity_z, dir_z) > 0f) {
                max_z = dir_z * max_velocity;
            }
            else {
                max_z = -dir_z * max_velocity;
            }
        }

        if(stop) {
            rigid.velocity = new Vector3(0, rigid.velocity.y, 0) + stop_x + stop_z;
        }
        if(keep_max) rigid.velocity = new Vector3(0, rigid.velocity.y, 0) + max_x + max_z;
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

    void OnTriggerEnter(Collider other) {
        if(other.name == "star") {
            other.gameObject.SetActive(false);
        }

        if(other.tag == "kasi") {
            transform.position = new Vector3(0f, 10f, -5f);
        }
    }

    Vector3 proj(Vector3 vec, Vector3 dir) { //dir_x is a unit vector
        return Vector3.Dot(vec, dir) * dir;
    }

    float vecConst(Vector3 vec, Vector3 dir) { // vec is expected to be oriented to the same direction as dir
        return Vector3.Dot(vec, dir);
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