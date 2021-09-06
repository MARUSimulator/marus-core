using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public Rigidbody rb;
    public Transform player;
    public float speed = 2f;
    public Vector3 AngleVelocity = new Vector3(0, 30, 0); // deg/sec


    // Update is called once per frame
    void FixedUpdate()
    {   
        
        if (Input.GetKey(KeyCode.W))
        {
            //Move forward
            Vector3 forceTemp = player.TransformDirection(0, 0, speed * Time.deltaTime);
            rb.AddForce(forceTemp, ForceMode.VelocityChange);
        }
        if(Input.GetKey(KeyCode.S))
        {
            //Move backward
            Vector3 forceTemp = player.TransformDirection(0, 0, -speed * Time.deltaTime);
            rb.AddForce(forceTemp, ForceMode.VelocityChange);
        }
        if(Input.GetKey(KeyCode.D))
        {
            //Rotate right
            Quaternion deltaRotation = Quaternion.Euler(AngleVelocity * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        if(Input.GetKey(KeyCode.A))
        {
            //Rotate left
            Quaternion deltaRotation = Quaternion.Euler(-AngleVelocity * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        if(Input.GetKey(KeyCode.Q))
        {
            rb.AddForce(0, speed * Time.deltaTime, 0, ForceMode.VelocityChange);
        }
        if(Input.GetKey(KeyCode.E))
        {
            rb.AddForce(0, -speed * Time.deltaTime, 0, ForceMode.VelocityChange);
        }

    }
}
