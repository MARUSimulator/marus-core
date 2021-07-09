using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUVPrimitiveController : MonoBehaviour
{

    public float linSpeed = 2f;
    public float rotSpeed = 700f;

    Transform _targetTransform;
    AgentManager agentManager;


    // Start is called before the first frame update
    void Awake()
    {
        _targetTransform = transform;
        agentManager = GameObject.FindObjectOfType<AgentManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (agentManager.activeAgent != gameObject)
            return;
        float dt = Time.fixedDeltaTime;
        UpdateMovement(dt);
    }

    void UpdateMovement(float dt)
    {
        var speed = linSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 3f;
        }

        _targetTransform.position += speed * _targetTransform.forward * (Input.GetKey(KeyCode.W) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.forward * (Input.GetKey(KeyCode.S) ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.up * (Input.GetKey(KeyCode.E) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.up * (Input.GetKey(KeyCode.Q) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.right * (Input.GetKey(KeyCode.A) ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.right * (Input.GetKey(KeyCode.D) ? 1 : 0) * dt;

        { // rotation
            float rotate = 0f;
            rotate += (Input.GetKey(KeyCode.X) ? 1 : 0);
            rotate -= (Input.GetKey(KeyCode.Y) ? 1 : 0);
            Vector3 ea = _targetTransform.eulerAngles;
            ea.y += 0.1f * rotSpeed * rotate * dt;
            _targetTransform.eulerAngles = ea;
        }
    }
}
