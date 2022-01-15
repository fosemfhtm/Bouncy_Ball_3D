using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraY : MonoBehaviour
{
    public GameObject player;
    public PlayerBall player_script;
    private float controlY;
    public bool ball_following = false;
    public float height = 10f;

    void Start()
    {
        player_script = (PlayerBall) player.GetComponent(typeof(PlayerBall));
        controlY = 0f;
    }

    void Update()
    {
        if(player.transform.position.y > player_script.floorY + height) {
            ball_following = true;
        }
        else {
            ball_following = false;
        }

        if(ball_following) {
            controlY = player.transform.position.y;
        }
        else {
            controlY = player_script.floorY + height;
        }

        transform.position = new Vector3(0, controlY, 0);
    }
}