using UnityEngine;
using System;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Line represented as set of two points using builtin LineRenderer object.
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

        private GameObject line;
        private LineRenderer lr;

        public Line()
        {
        }

        public Line(Vector3 start, Vector3 end)
        {
            StartPoint = start;
            EndPoint = end;
            InitLineRenderer();
        }

        public Line(Vector3 start, Vector3 end, Color color)
        {
            StartPoint = start;
            EndPoint = end;
            LineColor = color;
            InitLineRenderer();
        }

        public Line(Vector3 start, Vector3 end, Color color, float thickness)
        {
            StartPoint = start;
            EndPoint = end;
            LineColor = color;
            Thickness = thickness;
            InitLineRenderer();
        }

        /// <summary>
        /// Draw line
        /// </summary>
        public void Draw()
        {
            InitLineRenderer();
            lr.positionCount = 2;
            lr.SetPosition(0, StartPoint);
            lr.SetPosition(1, EndPoint);
        }

        /// <summary>
        /// Remove from scene and destroy object
        /// </summary>
        public void Destroy()
        {
            if (lr == null)
            {
                return;
            }
            UnityEngine.Object.Destroy(lr.gameObject);
        }

        private void InitLineRenderer()
        {
            if (line == null)
            {
                line = new GameObject();
                line.hideFlags = HideFlags.HideInHierarchy;
                line.AddComponent<LineRenderer>();
            }
            if (lr == null)
            {
                lr = line.GetComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            }

            float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(LineColor, 0.0f), new GradientColorKey(LineColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            lr.colorGradient = gradient;

            // set width of the renderer
            lr.startWidth = Thickness;
            lr.endWidth = Thickness;
            // set layer to visualisation initially
            line.layer = 6;
        }
    }
}
