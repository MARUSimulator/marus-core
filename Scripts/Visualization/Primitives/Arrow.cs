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
using System;
using Labust.Utils;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    ///
    /// </summary>
    public class Arrow : VisualElement
    {
        public float Radius = 0.05f;
        public float HeadRadius = 0.2f;
        public float HeadLength = 0.5f;
        public Vector3? StartPoint;
        public Vector3? EndPoint;

        public Color Color = Color.red;
        public Color HeadColor = Color.red;

        private GameObject parent;
        private GameObject arrow;
        private GameObject tail;
        private GameObject head;
        private bool destroyed = false;


        public Arrow()
        {
            Timestamp = DateTime.UtcNow;
            Lifetime = 0;
        }

        public Arrow(Vector3 start, Vector3 end, float radius, Color color) : this()
        {
            StartPoint = start;
            EndPoint = end;
            Radius = radius;
            Color = color;
        }

        public Arrow(Vector3 start, Vector3 end, float radius, Color color, float headRadius, Color headColor) : this(start, end, radius, color)
        {
            HeadColor = headColor;
            HeadRadius = headRadius;
        }

        /// <summary>
        /// Draw tail
        /// </summary>
        public override void Draw()
        {
            if (destroyed)
            {
                return;
            }

            if (arrow == null)
            {
                arrow = new GameObject("arrow");
                if (parent != null)
                {
                    arrow.transform.parent = parent.transform;
                }
                tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                UnityEngine.Object.Destroy(tail.GetComponent<CapsuleCollider>());
                Material newMat = new Material(Shader.Find("HDRP/Unlit"));
                tail.GetComponent<Renderer>().material = newMat;
                //tail.hideFlags = HideFlags.HideInHierarchy;
                tail.layer = LayerMask.NameToLayer("Visualization");
                tail.transform.SetParent(arrow.transform);

                head = new GameObject();
                head.AddComponent<MeshRenderer>();
                head.AddComponent<MeshFilter>();
            }

            // tail
            var _totalScale = Helpers.GetObjectScale(tail.transform, includeSelf:false);
            var _offset = (Vector3) (EndPoint - StartPoint);
            var _scale = new Vector3(Radius / _totalScale.x, _offset.magnitude / 2.0f / _totalScale.y, Radius / _totalScale.z);
            var position = StartPoint + (_offset / 2f);
            tail.transform.position = (Vector3) position;
            tail.transform.rotation = Quaternion.identity;
            tail.transform.up = _offset.normalized;
            tail.transform.localScale = _scale;
            tail.GetComponent<Renderer>().material.color = Color;

            //head
            var meshFilter = head.GetComponent<MeshFilter>();
            meshFilter.mesh = CreateConeMesh(10, HeadRadius, HeadLength);
            Material coneMat = new Material(Shader.Find("HDRP/Unlit"));
            head.GetComponent<Renderer>().material = coneMat;
            _scale = head.transform.localScale;
            head.transform.parent = arrow.transform;
            head.transform.localScale = _scale;
            head.transform.position = (Vector3) EndPoint;
            head.transform.rotation = Quaternion.identity;
            head.transform.up = (Vector3) _offset.normalized;
            head.GetComponent<Renderer>().material.color = HeadColor;
        }

        /// <summary>
        /// Remove from scene and destroy object
        /// </summary>
        public override void Destroy()
        {
            if (arrow == null)
            {
                return;
            }
            if (parent != null && parent.transform.childCount == 3)
            {
                UnityEngine.Object.Destroy(parent);
            }
            else
            {
                UnityEngine.Object.Destroy(tail);
                UnityEngine.Object.Destroy(head);
                UnityEngine.Object.Destroy(arrow);
            }
            arrow = null;
            destroyed = true;
        }

        public void SetRadius(float Radius)
        {
            if (Radius != 0)
            {
                this.Radius = Radius;
            }
        }

        public void SetHeadRadius(float Radius)
        {
            if (Radius != 0)
            {
                this.HeadRadius = Radius;
            }
        }

        public void SetHeadLength(float length)
        {
            if (length != 0)
            {
                this.HeadLength = length;
            }
        }

        public void SetColor(Color color)
        {
            this.Color = color;
        }

        public void SetHeadColor(Color color)
        {
            this.HeadColor = color;
        }

        public void SetParent(GameObject parent)
        {
            this.parent = parent;
        }

        private Mesh CreateConeMesh (int subdivisions, float radius, float height) {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0f);
            for(int i = 0, n = subdivisions - 1; i < subdivisions; i++) {
                float ratio = (float)i / n;
                float r = ratio * (Mathf.PI * 2f);
                float x = Mathf.Cos(r) * radius;
                float z = Mathf.Sin(r) * radius;
                vertices[i + 1] = new Vector3(x, 0f, z);

                uv[i + 1] = new Vector2(ratio, 0f);
            }
            vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);

            // construct bottom

            for(int i = 0, n = subdivisions - 1; i < n; i++) {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }

            // construct sides

            int bottomOffset = subdivisions * 3;
            for(int i = 0, n = subdivisions - 1; i < n; i++) {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
