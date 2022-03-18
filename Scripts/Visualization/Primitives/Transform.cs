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

using Marus.Utils;
using System;
using UnityEngine;

namespace Marus.Visualization.Primitives
{
    /// <summary>
    /// Implements visual representation of GameObject's transform.
    /// Lines are drawn from center of the object in each of the axes' direction.
    /// </summary>
    public class Transform : VisualElement
    {
        public UnityEngine.Transform MyTransform;
        public float LengthScale = 0.5f;
        public float LineThickness = 0.03f;

        private Line _xLine;
        private Line _yLine;
        private Line _zLine;

        private bool destroyed = false;

        /// <summary>
        /// Constructor which initializes visual element with gameobject's transform reference.
        /// </summary>
        /// <param name="transform"></param>
        public Transform(UnityEngine.Transform transform)
        {
            MyTransform = transform;

            var origin = MyTransform.TransformPoint(Vector3.zero);
            (var x, var y, var z) = CalculateAxisPoints();

            _xLine = new Line(origin, x, LineThickness);
            _xLine.LineColor = Color.red;
            _xLine.SetParent(transform.gameObject);

            _yLine = new Line(origin, y, LineThickness);
            _yLine.LineColor = Color.green;
            _yLine.SetParent(transform.gameObject);

            _zLine = new Line(origin, z, LineThickness);
            _zLine.LineColor = Color.blue;
            _zLine.SetParent(transform.gameObject);
            Lifetime = 0;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Draws transform
        /// </summary>
        public override void Draw()
        {
            if (destroyed)
            {
                return;
            }

            var origin = MyTransform.TransformPoint(Vector3.zero);
            (var x, var y, var z) = CalculateAxisPoints();

            _xLine.StartPoint = origin;
            _xLine.EndPoint = x;

            _yLine.StartPoint = origin;
            _yLine.EndPoint = y;

            _zLine.StartPoint = origin;
            _zLine.EndPoint = z;

            _xLine.Draw();
            _yLine.Draw();
            _zLine.Draw();
        }

        /// <summary>
        /// Destroys and removes transform
        /// </summary>
        public override void Destroy()
        {
            destroyed = true;
            _xLine.Destroy();
            _yLine.Destroy();
            _zLine.Destroy();
        }

        public void SetParent(GameObject parent)
        {
            _xLine.SetParent(parent);
            _yLine.SetParent(parent);
            _zLine.SetParent(parent);
        }

        private (Vector3, Vector3, Vector3) CalculateAxisPoints()
        {
            var totalScale = Helpers.GetObjectScale(MyTransform);
            var x_scale = 1 / totalScale.x;
            var y_scale = 1 / totalScale.y;
            var z_scale = 1 / totalScale.z;

            var origin = MyTransform.TransformPoint(Vector3.zero);
            var x = MyTransform.TransformPoint(Vector3.right * LengthScale * x_scale);
            var y = MyTransform.TransformPoint(Vector3.up * LengthScale * y_scale);
            var z = MyTransform.TransformPoint(Vector3.forward * LengthScale * z_scale);

            return (x, y, z);
        }
    }
}
