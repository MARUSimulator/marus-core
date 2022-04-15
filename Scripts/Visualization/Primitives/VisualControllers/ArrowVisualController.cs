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
using Marus.Visualization.Primitives;

namespace Marus.Visualization
{
    /// <summary>
    /// Used for line appearance customization
    /// </summary>
    public class ArrowVisualController : MonoBehaviour {

        public Arrow MyArrow;

        public Color Color = Color.red;

        [Range(0f, 4f)]
        public float Radius = 0.01f;

        public Color HeadColor = Color.red;

        [Range(0f, 4f)]
        public float HeadRadius = 0.02f;
        public float HeadLength = 0.5f;

        private Color _lastColor;
        private float _lastRadius;

        private Color _lastHeadColor;
        private float _lastHeadRadius;
        private float _lastHeadLength;

        private void Start() {
            Radius = MyArrow.Radius;
            Color = MyArrow.Color;

            HeadRadius = MyArrow.HeadRadius;
            HeadColor = MyArrow.HeadColor;
            HeadLength = MyArrow.HeadLength;
        }
        void Update() {
            if (_lastRadius != Radius)
            {
                MyArrow.SetRadius(Radius);
                _lastRadius = Radius;
            }

            if (_lastColor != Color)
            {
                MyArrow.SetColor(Color);
                _lastColor = Color;
            }

            if (_lastHeadRadius != HeadRadius)
            {
                MyArrow.SetHeadRadius(HeadRadius);
                _lastHeadRadius = HeadRadius;
            }

            if (_lastHeadColor != HeadColor)
            {
                MyArrow.SetHeadColor(HeadColor);
                _lastHeadColor = HeadColor;
            }

            if (_lastHeadLength != HeadLength)
            {
                MyArrow.SetHeadLength(HeadLength);
                _lastHeadLength = HeadLength;
            }
        }

        void OnDestroy() {
            MyArrow.Destroy();
        }
    }
}

