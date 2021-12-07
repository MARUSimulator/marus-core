using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    /// <summery>
    /// This class enables camera to follow the player.
    /// Therefore, camera will always remain in the specific 
    /// position in regards to the player and provide third 
    /// person perspective.
    /// <summery>

    public Transform player; 
    public Transform camera;
    public Vector3 shift = new Vector3(0, 10, -20);
    public float cameraAngle = 15f;

    void Update()
    {
        Vector3 shiftTransformed = player.TransformDirection(shift);
        camera.position = new Vector3(player.position.x + shiftTransformed.x, player.position.y + shiftTransformed.y, player.position.z + shiftTransformed.z);
        camera.eulerAngles = new Vector3(cameraAngle, player.eulerAngles.y , player.eulerAngles.z);
    }
}
