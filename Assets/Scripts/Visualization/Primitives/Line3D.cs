using UnityEngine;
using System;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Line represented as set of two points connected with a cylinder 3D primitive object.
    /// </summary>
    public class Line3D : VisualElement
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

        private GameObject cylinder;

        public Line3D()
        {
        }

        public Line3D(Vector3 start, Vector3 end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        public Line3D(Vector3 start, Vector3 end, float width)
        {
            StartPoint = start;
            EndPoint = end;
            Thickness = width;
        }

        public Line3D(Vector3 start, Vector3 end, float width, Color _color)
        {
            StartPoint = start;
            EndPoint = end;
            Thickness = width;
            LineColor = _color;
        }

        /// <summary>
        /// Draw line
        /// </summary>
        public void Draw()
        {
            if (cylinder != null)
            {
                return;
            }

            cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var offset = EndPoint - StartPoint;
            var scale = new Vector3(Thickness, offset.magnitude / 2.0f, Thickness);
            var position = StartPoint + (offset / 2.0f);

            cylinder.transform.position = position;
            cylinder.transform.rotation = Quaternion.identity;
            cylinder.transform.up = offset;
            cylinder.transform.localScale = scale;
            cylinder.GetComponent<Renderer>().material.color = LineColor;
            cylinder.hideFlags = HideFlags.HideInHierarchy;
            UnityEngine.Object.Destroy(cylinder.GetComponent<CapsuleCollider>());
            cylinder.layer = 6;
        }

        /// <summary>
        /// Remove from scene and destroy object
        /// </summary>
        public void Destroy()
        {
            if (cylinder == null)
            {
                return;
            }
            UnityEngine.Object.Destroy(cylinder);
        }

        /// <summary>
        /// Sets line thickness
        /// </summary>
        public void SetThickness(float width)
        {
            if (cylinder == null)
            {
                return;
            }
            var oldScale = cylinder.transform.localScale;
            cylinder.transform.localScale = new Vector3(width, oldScale.y, width);
        }

        /// <summary>
        /// Sets line color
        /// </summary>
        public void SetColor(Color color)
        {
            if (cylinder == null)
            {
                return;
            }
            cylinder.GetComponent<Renderer>().material.color = color;
        }
    }
}
