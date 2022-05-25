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

/// <summary>
/// This controller is for using keyboard to control desired object
///
/// It is under control of agent manager.
/// </summary>
public class AUVPrimitiveController : MonoBehaviour
{

    public float linSpeed = 2f;
    public float rotSpeed = 700f;
    private readonly List<KeyCode> keycodes = new List<KeyCode>{KeyCode.W, KeyCode.S, KeyCode.E, KeyCode.Q, KeyCode.A, KeyCode.D, KeyCode.X, KeyCode.Y};

    Transform _targetTransform;


    // Start is called before the first frame update
    void Awake()
    {
        _targetTransform = transform;
        AgentManager.Instance.Register(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (AgentManager.Instance.activeAgent != gameObject)
            return;

        var speed = linSpeed;


        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 3f;
        }

        float dt = Time.fixedDeltaTime;

        foreach (var item in keycodes)
        {
            if (Input.GetKey(item))
            UpdateMovement(dt, item, speed);
        }
    }

    void UpdateMovement(float dt, KeyCode key, float speed)
    {
        _targetTransform.position += speed * _targetTransform.forward * (key == KeyCode.W ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.forward * (key == KeyCode.S ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.up * (key == KeyCode.E ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.up * (key == KeyCode.Q ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.right * (key == KeyCode.A ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.right * (key == KeyCode.D ? 1 : 0) * dt;

        { // rotation
            float rotate = 0f;
            rotate += (key == KeyCode.X ? 1 : 0);
            rotate -= (key == KeyCode.Y ? 1 : 0);
            Vector3 ea = _targetTransform.eulerAngles;
            ea.y += rotSpeed * rotate * dt;
            _targetTransform.eulerAngles = ea;
        }
    }
}
