// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CREST_OCEAN
using Crest;
#endif

public class FPVController : MonoBehaviour
{
    public float walkingSpeed = 1500.0f;
    public float runningSpeed = 2000.0f;
    public float lookSpeed = 20.0f;
    
    Transform _targetTransform;
    Rigidbody _rigidBody;
    
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    float rotationY = 0;
    
    float surfaceLevel = 0;
    
    public bool allowSidewayMotion = false;
    public float sidewayMotionSpeedMul = 1.0f;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        _targetTransform = transform;
        _rigidBody = GetComponent<Rigidbody>();
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // Start is called before the first frame update
    void Awake()
    {
        AgentManager.Instance.Register(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (AgentManager.Instance.activeAgent != gameObject)
            return;
        float dt = Time.fixedDeltaTime;
        UpdateMovement(dt);
    }

    void UpdateMovement(float dt)
    {
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Player and Camera rotation
        if (canMove)
        {
            if(_rigidBody)
            {
                // reset velocities due collisions
                _rigidBody.angularVelocity = new Vector3(0,0,0);
                
                // stabilize z axis
                Quaternion targetRotation = Quaternion.Euler(_targetTransform.eulerAngles.x, _targetTransform.eulerAngles.y, 0);
                
                _targetTransform.rotation = Quaternion.Slerp(_targetTransform.rotation, targetRotation,  dt * 50);

                // drive vehicle
                if(allowSidewayMotion)
                {
                    _rigidBody.AddForce((isRunning ? runningSpeed : walkingSpeed) * _targetTransform.right * Input.GetAxis("Horizontal") * dt * sidewayMotionSpeedMul);
                    
                    _rigidBody.AddForce((isRunning ? runningSpeed : walkingSpeed) * _targetTransform.forward * Input.GetAxis("Vertical") * dt);
                }
                else
                {
                    _rigidBody.AddForce((isRunning ? runningSpeed : walkingSpeed) * _targetTransform.forward * (Input.GetAxis("Vertical") > 0 ? Input.GetAxis("Vertical") : 0) * dt);
                }
            }
            else 
            {
                _targetTransform.position += (isRunning ? runningSpeed : walkingSpeed) * _targetTransform.forward * (Input.GetKey(KeyCode.W) ? 1 : 0) * dt;
            }
            Vector3 eul = new Vector3(-Input.GetAxis("Mouse Y") * lookSpeed * dt,0.0f,0.0f);
            // rotate pitch
            _targetTransform.Rotate(eul);
            eul.x = 0;
            eul.y = Input.GetAxis("Mouse X") * lookSpeed * dt;
            // rotate global yaw
            _targetTransform.Rotate(eul, Space.World);
#if CREST_OCEAN
            if(OceanRenderer.Instance)
            {
                _targetTransform.position += OceanRenderer.Instance.ViewerHeightAboveWater > 0 ? new Vector3(0,-OceanRenderer.Instance.ViewerHeightAboveWater,0) : new Vector3(0,0,0);
            }
            else
            {
#endif
                // disable driving above water level
                if(_targetTransform.position.y >= surfaceLevel)
                {
                    _targetTransform.position += new Vector3(0,surfaceLevel-_targetTransform.position.y,0);
                }
#if CREST_OCEAN
            }
#endif
        }
    }
}
