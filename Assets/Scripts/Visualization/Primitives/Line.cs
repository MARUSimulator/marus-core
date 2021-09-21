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
        public float Thickness = 0.01f;

        /// <summary>
        /// Line color
        /// </summary>
        public Color LineColor = Color.yellow;

        GameObject line;

        private GameObject parent;

        private bool destroyed = false;

        public Line(Vector3 start, Vector3 end)
        {
            StartPoint = start;
            EndPoint = end;
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
        public void Draw()
        {
            if (destroyed)
            {
                return;
            }

            if (line == null)
            {
                line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                UnityEngine.Object.Destroy(line.GetComponent<CapsuleCollider>());
                line.hideFlags = HideFlags.HideInHierarchy;
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
            // line.layer = 6;

        }

        /// <summary>
        /// Remove from scene and destroy object
        /// </summary>
        public void Destroy()
        {
            if (line == null)
            {
                return;
            }
            UnityEngine.Object.Destroy(line);
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
