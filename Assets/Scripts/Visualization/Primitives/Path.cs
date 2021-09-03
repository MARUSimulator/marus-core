using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Draw path as by-part linear.
    /// If Transform[] is given in the constructor, follow the transform origins of every point in every frame
    /// </summary>
    public class Path : VisualElement
    {
        /// <summary>
        /// Line thickness
        /// </summary>
        public float LineThickness = 0.05f;
        /// <summary>
        /// Line color
        /// </summary>
        public Color LineColor = Color.yellow;
        /// <summary>
        /// Point size
        /// </summary>
        public float PointSize = 0.1f;
        /// <summary>
        /// Point color
        /// </summary>
        public Color PointColor = Color.yellow;

        /// <summary>
        /// List containing all path points
        /// </summary>
        private List<Point> _points;

        private GameObject line;
        private LineRenderer lr;

        /// <summary>
        /// Empty constructor which initializes empty _points list and empty 
        /// _startPointLineDict dictionary
        /// </summary>
        public Path()
        {
            _points = new List<Point>();
            InitLineRenderer();
        }

        /// <summary>
        /// Constructor which initializes empty _points list and empty
        /// _startPointLineDict dictionary and sets line color and line thickness.
        /// </summary>
        public Path(float LineThickness, Color lc)
        {
            this.LineThickness = LineThickness;
            _points = new List<Point>();
            LineColor = lc;
            InitLineRenderer();
        }

        /// <summary>
        /// Constructor which initializes path from list of points.
        /// </summary>
        /// <param name="pointsInWorld">List of points to create a path from.</param>
        public Path(List<Vector3> pointsInWorld)
        {
            _points = new List<Point>();
            foreach (Vector3 pointInWorld in pointsInWorld)
            {
                _points.Add(new Point(pointInWorld));
            }
            InitLineRenderer();
        }

        /// <summary>
        /// Constructor which initializes path from given points and sets all the path properties.
        /// </summary>
        /// <param name="pointsInWorld">List of points</param>
        /// <param name="_PointSize">Point size</param>
        /// <param name="_PointColor">Point color</param>
        /// <param name="_LineThickness">Line thickness</param>
        /// <param name="_LineColor">Line color</param>
        public Path(List<Vector3> pointsInWorld, float _PointSize, Color _PointColor, float _LineThickness, Color _LineColor)
        {
            _points = new List<Point>();
            PointSize = _PointSize;
            PointColor = _PointColor;
            LineThickness = _LineThickness;
            LineColor = _LineColor;
            foreach (Vector3 pointInWorld in pointsInWorld)
            {
                _points.Add(new Point(pointInWorld, _PointSize, _PointColor));
            }
            InitLineRenderer();
        }


        /// <summary>
        /// Initializes LineRenderer component and sets material, color and thickness of line.
        /// </summary>
        private void InitLineRenderer()
        {
            line = new GameObject();
            line.hideFlags = HideFlags.HideInHierarchy;
            line.AddComponent<LineRenderer>();
            lr = line.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

            float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(LineColor, 0.0f), new GradientColorKey(LineColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            lr.colorGradient = gradient;

            // set width of the renderer
            lr.startWidth = LineThickness;
            lr.endWidth = LineThickness;
            // set layer to visualisation initially
            line.layer = 6;
        }

        /// <summary>
        /// Adds a single point to existing path.
        /// </summary>
        /// <param name="point">Point to add</param>
        public void AddPointToPath(Vector3 point)
        {
            _points.Add(new Point(point));
        }

        // <summary>
        /// Adds a single point to existing path with given size and color.
        /// </summary>
        /// <param name="point">Point to add</param>
        /// <param name="PointSize">Point size</param>
        /// <param name="PointColor">Point color</param>
        public void AddPointToPath(Vector3 point, float PointSize, Color PointColor)
        {
            _points.Add(new Point(point, PointSize, PointColor));
        }

        /// <summary>
        /// Checks if any point is older than limit and removes if needed.
        /// </summary>
        /// <param name="limit">Point age limit in seconds</param>
        public void RefreshAndFade(float limit)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                if (DateTime.UtcNow.Subtract(_points[i].Timestamp).TotalSeconds > limit)
                {
                    _points[i].Destroy();
                    _points.Remove(_points[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Draws path
        /// </summary>
        public void Draw()
        {
            for (var i = 0; i < _points.Count; i++)
            {
                lr.positionCount = i + 1;
                _points[i].Draw();
                lr.SetPosition(i, _points[i].Position);
            }
        }

        /// <summary>
        /// Destroys path object
        /// </summary>
        public void Destroy()
        {
            foreach (Point p in _points)
            {
                p.Destroy();
            }
            _points.Clear();

            if (lr != null)
            {
                UnityEngine.Object.Destroy(lr.gameObject);
            }
        }

        public void SetPointSize(float size)
        {
            foreach (Point p in _points)
            {
                p.SetSize(size);
            }
        }

        public void SetPointColor(Color color)
        {
            foreach (Point p in _points)
            {
                p.SetColor(color);
            }
        }

        public void SetLineColor(Color color)
        {
            float alpha = 1.0f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(LineColor, 0.0f), new GradientColorKey(LineColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
            lr.colorGradient = gradient;
        }

        public void SetLineThickness(float LineThickness)
        {
            lr.startWidth = LineThickness;
            lr.endWidth = LineThickness;
        }
    }
}
