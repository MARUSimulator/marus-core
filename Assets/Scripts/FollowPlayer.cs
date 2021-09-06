using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Transform player; 
    public Transform camera;
    public Vector3 shift = new Vector3(0, 10, -20);
    public float cameraAngle = 15f;
    // Update is called once per frame
    void Update()
    {
        
        Vector3 shiftTransformed = player.TransformDirection(shift);
        camera.position = new Vector3(player.position.x + shiftTransformed.x, player.position.y + shiftTransformed.y, player.position.z + shiftTransformed.z);
        camera.eulerAngles = new Vector3(cameraAngle, player.eulerAngles.y , player.eulerAngles.z);

    }
}
