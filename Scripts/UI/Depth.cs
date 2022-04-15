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

using UnityEngine;
using UnityEngine.UI;

namespace Marus.UI
{
    /// <summary>
    /// UI script that controls the UI depth information
    /// </summary>
    public class Depth : MonoBehaviour
    {

        public GameObject Player;

        Text _depthText;
        string depthPrefix;

        // Start is called before the first frame update
        void Start()
        {
            _depthText = GetComponent<Text>();
            depthPrefix = _depthText.text;
        }

        // Update is called once per frame
        void Update()
        {
            _depthText.text = $"{depthPrefix} {Mathf.Max(-Player.transform.position.y, 0f):0.##}";
        }
    }
}