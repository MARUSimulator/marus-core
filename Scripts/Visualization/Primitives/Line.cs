using UnityEngine;
using System;
using Labust.Utils;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Line represented as set of two points connected with a cylinder 3D primitive object.
    /// </summary>
    public class Line : VisualElement
    {
        /// <summary>
        /// Starting point of the line
        /// </summary>
        public Vector3 StartPoint;

        /// <summary>
        /// Ending point of the line
        /// </summary>
        public Vector3 EndPoint;

        /// <summary>
        /// Line thickness
        /// </summary>
        public float Thickness = 0.05f;

        /// <summary>
        /// Line color
        /// </summary>
        public Color LineColor = Color.red;

        GameObject line;

        private GameObject parent;

        private bool destroyed = false;

        public Line(Vector3 start, Vector3 end)
        {
            StartPoint = start;
            EndPoint = end;
            Timestamp = DateTime.UtcNow;
            Lifetime = 0;
        }

        public Line(Vector3 start, Vector3 end, float width) : this(start, end)
        {
            Thickness = width;
        }

        public Line(Vector3 start, Vector3 end, float width, Color _color) : this(start, end, width)
        {
            LineColor = _color;
        }

        /// <summary>
        /// Draw line
        /// </summary>
        public override void Draw()
        {
            if (destroyed)
            {
                return;
            }

            if (line == null)
            {
                line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                UnityEngine.Object.Destroy(line.GetComponent<CapsuleCollider>());
                Material newMat = new Material(Shader.Find("HDRP/Unlit"));
                line.GetComponent<Renderer>().material = newMat;
                line.hideFlags = HideFlags.HideInHierarchy;
                line.layer = LayerMask.NameToLayer("Visualization");
                line.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Lines");
                line.isStatic = true;
            }
            if (parent != null)
            {
                line.transform.SetParent(parent.transform);
            }

            var totalScale = Helpers.GetObjectScale(line.transform, includeSelf:false);

            var offset = EndPoint - StartPoint;
            var scale = new Vector3(Thickness / totalScale.x, offset.magnitude / 2.0f / totalScale.y, Thickness / totalScale.z);
            var position = StartPoint + (offset / 2.0f);
            line.transform.position = position;
            line.transform.rotation = Quaternion.identity;
            line.transform.up = offset.normalized;
            line.transform.localScale = scale;
            line.GetComponent<Renderer>().material.color = LineColor;
        }

        /// <summary>
        /// Remove from scene and destroy object
        /// </summary>
        public override void Destroy()
        {
            if (line == null)
            {
                return;
            }
            if (parent != null && parent.transform.childCount == 1)
            {
                UnityEngine.Object.Destroy(parent);
            }
            else
            {
                UnityEngine.Object.Destroy(line);
            }
            line = null;
            destroyed = true;
        }

        /// <summary>
        /// Sets line thickness
        /// </summary>
        public void SetThickness(float width)
        {
            Thickness = width;
        }

        /// <summary>
        /// Sets line color
        /// </summary>
        public void SetColor(Color color)
        {
            LineColor = color;
        }

        public void SetParent(GameObject parent)
        {
            this.parent = parent;
        }
    }
}
