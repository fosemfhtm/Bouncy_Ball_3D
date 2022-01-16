using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBall : MonoBehaviour
{
    public GameObject camera;
    public camera camera_script;

    public GameObject dynamicObject;
    public DynamicObjParent dynamicObjParent;

    public float factor = 0.7f;
    public float max_velocity = 17f;
    public float friction_force = 0.7f;
    public float velocity_lowerbound = 0.36f;

    public float DoubleClickTerm = 0.35f;
    public float DashForce = 30f;
    public float JumpForce = 40f;
    private bool isOneClick_h = false, isOneClick_v = false, isOneJump = false;
    private double PrevClickTime_h = 0, PrevClickTime_v = 0, PrevClickTime_j = 0;
    
    private bool dash = false;
    private bool jump = false;
    private bool dashing = false, shooting = false; // 두 값이 모두 true일 수 없도록 구현.
    private float prev_h = 0, prev_v = 0, prek_h = 0, prek_v = 0;
    private bool prek_j = false;

    public float floorY = 0f;
    public Vector3 respawn_pos;
    private bool bounce_start = false, bounce_finish = false;

    Rigidbody rigid;
    Color rigid_color;

    private Vector3 dir_x, dir_z;
    private float jump_velocity = 37f;
    private float bound_error = 0.5f;
    private float pushing_factor = 1.8f;
    
    void Awake() {
        rigid = GetComponent<Rigidbody>();
        rigid_color = GetComponent<Renderer>().material.GetColor("_Color");

        Vector3 dir = camera.transform.rotation * Vector3.forward;
        dir.y = 0f;
        dir_z = (dir).normalized;
        dir_x = -Vector3.Cross(dir_z, new Vector3(0, 1f, 0));

        camera_script = (camera) camera.GetComponent(typeof(camera));
        dynamicObjParent = (DynamicObjParent) dynamicObject.GetComponent(typeof(DynamicObjParent));
        bounce_start = false;
        bounce_finish = false;

        transform.position = respawn_pos;
    }

    void Update() {
        if(transform.position.y < -10f) {
            respawn();
        }
    }
    
    void FixedUpdate() {        
        Ray ray = new Ray();
        ray.origin = transform.position;
        ray.direction = -Vector3.up;

        RaycastHit HitOut;
        if(Physics.Raycast(ray, out HitOut)) {
            floorY = HitOut.transform.gameObject.transform.position.y + HitOut.transform.gameObject.GetComponent<Collider>().bounds.size.y / 2f; // 충돌 블록 상하, 좌우 대칭 가정.
        }
        else {
            floorY = -10f;
        }
        
        float hSmooth = Input.GetAxisRaw("Horizontal");
        float vSmooth = Input.GetAxisRaw("Vertical");
        bool isJump = Input.GetKey(KeyCode.Space);
        
        Vector3 dashDir = new Vector3(0, 0, 0);
        
        float h = 0, v = 0;
        bool j = false;

        if(prek_h == 0 && hSmooth != 0) {
            h = hSmooth;
        }

        if(prek_v == 0 && vSmooth != 0) {
            v = vSmooth;
        }

        if(!prek_j && isJump) {
            j = isJump;
        }

        if(isOneClick_h && (Time.time - PrevClickTime_h) > DoubleClickTerm) {
            // Debug.Log("One Click!!");
            prev_h = 0;
            isOneClick_h = false;
        }

        if(isOneClick_v && (Time.time - PrevClickTime_v) > DoubleClickTerm) {
            // Debug.Log("One Click!!");
            prev_v = 0;
            isOneClick_v = false;
        }
        
        if(!dashing) {
            if (h!=0 || v!=0) {
                dashDir = isDoubleClick(h, v);

                if(!(dashDir.x == 0 && dashDir.y == 0 && dashDir.z == 0) && dash) {
                    GetComponent<Renderer>().material.SetColor("_Color", rigid_color);
                    rigid.velocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
                    rigid.AddForce(dashDir * DashForce, ForceMode.Impulse);
                    dash = false;
                    rigid.useGravity = true;

                    dashing = true;
                    shooting = false;
                }

                if (shooting) {
                    shooting = false;
                    rigid.useGravity = true;
                }
            }

            if(!shooting) {
                smoothMoving(hSmooth, vSmooth);
            }
        }
        else if((prev_v != 0 && prev_v * v < 0) || (prev_h != 0 && prev_h * h < 0)) { // dash before rebound
            rigid.AddForce(h*friction_force*dir_x + v*friction_force*dir_z, ForceMode.Impulse);
            prev_v = 0;
            prev_h = 0;
            dashing = false;
        }

        if(isOneJump && (Time.time - PrevClickTime_j) > DoubleClickTerm) {
            isOneJump = false;
        }

        if(j) {
            if(isDoubleJump() && jump) {
                jump = false;
                GetComponent<Renderer>().material.SetColor("_Color", rigid_color);
                rigid.velocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
                rigid.AddForce(new Vector3(0, 1f, 0) * JumpForce, ForceMode.Impulse);
                
                rigid.useGravity = true;
                shooting = false;
            }
        }

        prek_j = isJump;
        prek_h = hSmooth;
        prek_v = vSmooth;
    }

    void smoothMoving (float h, float v) {
        Vector3 dir = camera.transform.rotation * Vector3.forward;
        dir.y = 0f;
        dir_z = (dir).normalized;
        dir_x = -Vector3.Cross(dir_z, new Vector3(0, 1f, 0));

        Vector3 velocity_x = proj(rigid.velocity, dir_x);
        Vector3 velocity_z = proj(rigid.velocity, dir_z);

        bool stop = false, change_dir_x = false, change_dir_z = false;
        Vector3 stop_x = velocity_x, stop_z = velocity_z;
        Vector3 max_x = velocity_x, max_z = velocity_z;

        // Debug.Log(Vector3.Magnitude(velocity_x));
        // Debug.Log(Vector3.Magnitude(velocity_z));

        if(Vector3.Magnitude(velocity_x) >= velocity_lowerbound && h * vecConst(velocity_x, dir_x) < 0f) {
            change_dir_x = true;
        }

        if(Vector3.Magnitude(velocity_z) >= velocity_lowerbound && v * vecConst(velocity_z, dir_z) < 0f) {
            change_dir_z = true;
        }

        if(h!=0) { // 원래대로 하면 |velocity_x| == 10f 일 때, 또는 계산 오차로 그보다 클 때, 방향 전환 시 이 if문으로 안 들어와서 감속이 안 됨 >> 키씹
            float force_control;

            if(change_dir_x) { // reverse direction
                force_control = pushing_factor * 2.5f*h*factor;
            }
            else { // same direction
                force_control = pushing_factor * h*factor;

                if(Vector3.Magnitude(velocity_x + dir_x * force_control) >= max_velocity) {
                    force_control = h*(max_velocity - Vector3.Magnitude(velocity_x)); // 어차피 h==0이면 이 구문에서 힘이 들어오지 않음.
                }
            }

            rigid.AddForce(dir_x * force_control, ForceMode.Impulse);
        }
        
        if(v!=0) {
            float force_control;

            if(change_dir_z) { // reverse direction
                force_control = pushing_factor * 2.5f*v*factor;
            }
            else { // same direction
                force_control = pushing_factor * v*factor;

                if(Vector3.Magnitude(velocity_z + dir_z * force_control) >= max_velocity) {
                    force_control = v*(max_velocity - Vector3.Magnitude(velocity_z)); // 어차피 v==0이면 이 구문에서 힘이 들어오지 않음.
                }
            }

            rigid.AddForce(dir_z * force_control, ForceMode.Impulse);
        }

        if(h==0) {
            if(Vector3.Magnitude(velocity_x) >= velocity_lowerbound) {
                if(vecConst(velocity_x, dir_x) > 0f) {
                    rigid.AddForce(-friction_force * dir_x, ForceMode.Impulse);
                }
                else {
                    rigid.AddForce(friction_force * dir_x, ForceMode.Impulse);
                }
            }
            else {
                stop = true;
                stop_x = dir_x * 0f;
            }
        }

        if(v==0) {
            if(Vector3.Magnitude(velocity_z) >= velocity_lowerbound) {
                if(vecConst(velocity_z, dir_z) > 0f) {
                    rigid.AddForce(-friction_force * dir_z, ForceMode.Impulse);
                }
                else {
                    rigid.AddForce(friction_force * dir_z, ForceMode.Impulse);
                }
            }
            else {
                stop = true;
                stop_z = dir_z * 0f;
            }
        }

        if(stop) {
            rigid.velocity = new Vector3(0, rigid.velocity.y, 0) + stop_x + stop_z;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
        prev_v = 0;
        prev_h = 0;
        dashing = false;
        shooting = false;
        rigid.useGravity = true;

        if(collision.gameObject.tag == "JumpBlock") {
            ExecuteJumpBounding(collision);        
        }
        else {
            ExecuteReBounding(collision);
        }
        //Debug.Log(collision.gameObject.GetComponent<Collider>().bounds.size); //부딪히는 "바닥"은 모두 위쪽 축이 y이며 상하/좌우 대칭인 블록이라고 가정.
        //Debug.Log(collision.gameObject);

        if(collision.gameObject.tag == "fragile_block") {
            collision.gameObject.SetActive(false);
        }
    }
    
    
    void ExecuteReBounding(Collision collision) // 키씹 및 과속의 원인 2: 4번째 줄 안 쓰면 x, z 방향으로도 bouncing force가 적용된다.
    {
        // ContactPoint cp = collision.GetContact(0);
        // Vector3 dir = rigid.position - cp.point;
        // Debug.Log(dir);
        float ctoc_x = Mathf.Abs(collision.transform.position.x - transform.position.x);
        float ctoc_z = Mathf.Abs(collision.transform.position.z - transform.position.z);
        if(ctoc_x < (collision.gameObject.GetComponent<Collider>().bounds.size.x / 2f) + bound_error && ctoc_z < (collision.gameObject.GetComponent<Collider>().bounds.size.z / 2f) + bound_error) {
            rigid.velocity = new Vector3(rigid.velocity.x, jump_velocity, rigid.velocity.z);
        }
        // dir.Set(dir.x * 0, dir.y, dir.z * 0);
        // GetComponent<Rigidbody>().AddForce((dir).normalized * 1500f);
    }

    void ExecuteJumpBounding(Collision collision) // ?                  2: 4  °            x, z        ε  bouncing force       ? .
    {
        // ContactPoint cp = collision.GetContact(0);
        // Vector3 dir = rigid.position - cp.point;
        // // Debug.Log(dir);
        // dir.Set(dir.x * 0, dir.y, dir.z * 0);
        // GetComponent<Rigidbody>().AddForce((dir).normalized * 2500f);

        float ctoc_x = Mathf.Abs(collision.transform.position.x - transform.position.x);
        float ctoc_z = Mathf.Abs(collision.transform.position.z - transform.position.z);
        if(ctoc_x < (collision.gameObject.GetComponent<Collider>().bounds.size.x / 2f) + bound_error && ctoc_z < (collision.gameObject.GetComponent<Collider>().bounds.size.z / 2f) + bound_error) {
            rigid.velocity = new Vector3(rigid.velocity.x, jump_velocity * 1.4f, rigid.velocity.z);
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "star") {
            other.gameObject.SetActive(false);
        }

        if(other.tag == "kasi") {
            bounce_start = false;
            bounce_finish = false;
            respawn();
        }

        if(other.tag == "dash_item") {
            init_jump_var();
            dash = true;
            GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0, 0));
            other.gameObject.SetActive(false);
        }

        if(other.tag == "jump_item") {
            init_dash_var();
            jump = true;
            GetComponent<Renderer>().material.SetColor("_Color", new Color(0.314f, 0.071f, 0.024f));
            other.gameObject.SetActive(false);
        }

        if(other.tag == "CanonBlock") {
            ExecuteShooting(other);
        }
    }

    void ExecuteShooting(Collider other) {
        shooting = true;
        dashing = false;
        gameObject.transform.position = other.transform.position;

        rigid.velocity = Vector3.zero;
        rigid.useGravity = false;

        float canon_rotation = other.transform.eulerAngles.y;
        
        if(canon_rotation == 0f) {
            rigid.velocity = Vector3.right * 20;
        }
        else if(canon_rotation == 90f) {
            rigid.velocity = Vector3.back * 20;
        }
        else if(canon_rotation == 180f) {
            rigid.velocity = Vector3.left * 20;
        }
        else {
            rigid.velocity = Vector3.forward * 20;
        }
    }

    Vector3 proj(Vector3 vec, Vector3 dir) { //dir_x is a unit vector
        return Vector3.Dot(vec, dir) * dir;
    }

    float vecConst(Vector3 vec, Vector3 dir) { // vec is expected to be oriented to the same direction as dir
        return Vector3.Dot(vec, dir);
    }

    bool isDoubleJump() {
        bool ret = false;

        if(!isOneJump) {
            PrevClickTime_j = Time.time;
            isOneJump = true;
        }
        else if(isOneJump && (Time.time - PrevClickTime_j) <= DoubleClickTerm) {
            ret = true;
            isOneJump = false;
        }

        return ret;
    }

    Vector3 isDoubleClick(float h, float v) { // 좌클릭 더블
        Vector3 isDouble = new Vector3(0,0,0);
        // 둘 다 눌리면 v 우선 (앞, 뒤)
        
        if(v!=0) {
            if(!isOneClick_v) {
                // Debug.Log("One Click!!");
                PrevClickTime_v = Time.time;
                isOneClick_v = true;
                prev_v = v;
            }
            else if(isOneClick_v && (Time.time - PrevClickTime_v) <= DoubleClickTerm) {
                if(v == prev_v) {
                    // Debug.Log("Double Click!!");
                    isOneClick_h = false;
                    isOneClick_v = false;
                    prev_h = 0;
                    isDouble = dir_z * v;
                }
                else {
                    // Debug.Log("One Click!!");
                    prev_v = 0; //Fixed Update의 if dashing 구문에서 두 case가 엮여 있으니, 두 case가 어떻게든 중첩되지 않도록 설정.
                    isOneClick_v = false;
                }
            }
        }
        
        if(h!=0) {
            if(!isOneClick_h) {
                // Debug.Log("One Click!!");
                PrevClickTime_h = Time.time;
                isOneClick_h = true;
                prev_h = h;
            }
            else if(isOneClick_h && (Time.time - PrevClickTime_h) <= DoubleClickTerm) {
                if(h == prev_h) {
                    // Debug.Log("Double Click!!");
                    isOneClick_h = false;
                    isOneClick_v = false;
                    prev_v = 0;
                    isDouble = dir_x * h;
                }
                else {
                    // Debug.Log("One Click!!");
                    prev_h = 0;
                    isOneClick_h = false;
                }
            }
        }

        return isDouble;
    }

    void init_item_var() {
        init_dash_var();
        init_jump_var();
        init_canon_var();
    }

    void init_dash_var() {
        isOneClick_h = false;
        isOneClick_v = false;
        PrevClickTime_h = 0;
        PrevClickTime_v = 0;
        dash = false;
        dashing = false;
        prev_h = 0;
        prev_v = 0;
        prek_h = 0;
        prek_v = 0;
    }

    void init_jump_var() {
        isOneJump = false;
        PrevClickTime_j = 0;
        jump = false;
        prek_j = false;
    }

    void init_canon_var() {
        shooting = false;
    }

    void respawn() {
        GetComponent<Renderer>().material.SetColor("_Color", rigid_color);
        init_item_var();
        transform.position = respawn_pos;
        rigid.velocity = new Vector3(0f, 0f, 0f);
        dynamicObjParent.ActivateAllChildren();
    }

}