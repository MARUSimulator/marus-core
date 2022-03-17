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
using System.Linq;
using UnityEngine;

namespace Labust.UI
{
    /// <summary>
    /// UI script that controls the UI compass
    /// </summary>
    public class Compass : MonoBehaviour
    {

        public GameObject Player;

        Vector3 _northDirection;
        Transform _needle;

        // Start is called before the first frame update
        void Awake()
        {
            _needle = transform.GetChild(0);
        }

        // Update is called once per frame
        void Update()
        {
            if (Player != null)
            {
                _needle.rotation = Quaternion.Euler(0, 0, Player.transform.eulerAngles.y);
            }
        }
    }
}