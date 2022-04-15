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
