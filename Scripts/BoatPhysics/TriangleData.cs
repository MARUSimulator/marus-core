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

namespace Marus
{
    //To save space so we don't have to send millions of parameters to each method
    public struct TriangleData
    {
        //The corners of this triangle in global coordinates
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

        //The center of the triangle in global coordinates
        public Vector3 center;

        //The normal to the triangle
        public Vector3 normal;

        //The area of the triangle
        public float area;

        public static float GetTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
             return (Vector3.Distance(p1, p2) * Vector3.Distance(p3, p1) * Mathf.Sin(Vector3.Angle(p2 - p1, p3 - p1) * Mathf.Deg2Rad)) / 2f;
        }

        public TriangleData(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            //Center of the triangle
            this.center = (p1 + p2 + p3) / 3f;

            //Normal to the triangle
            this.normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

            //Area of the triangle
            float a = Vector3.Distance(p1, p2);

            float c = Vector3.Distance(p3, p1);

            this.area = GetTriangleArea(p1, p2, p3);
        }


    }
}