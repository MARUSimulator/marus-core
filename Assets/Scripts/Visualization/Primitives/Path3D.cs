using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Draw path as by-part linear with points connected with a cylinder 3D primitive object.
    /// </summary>
    public class Path3D : VisualElement
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

        /// <summary>
        /// Helper dictionary which references Line3D object by it's starting point
        /// </summary>
        private Dictionary<Point, Line3D> _startPointLineDict;

        /// <summary>
        /// Empty constructor which initializes empty _points list and empty 
        /// _startPointLineDict dictionary
        /// </summary>
        public Path3D()
        {
            _points = new List<Point>();
            _startPointLineDict = new Dictionary<Point, Line3D>();
        }

        /// <summary>
        /// Constructor which initializes empty _points list and empty
        /// _startPointLineDict dictionary and sets line color and line thickness.
        /// </summary>
        public Path3D(float LineThickness, Color lc)
        {
            this.LineThickness = LineThickness;
            _points = new List<Point>();
            _startPointLineDict = new Dictionary<Point, Line3D>();
            LineColor = lc;
        }

        /// <summary>
        /// Constructor which initializes path from list of points.
        /// </summary>
        /// <param name="pointsInWorld">List of points to create a path from.</param>
        public Path3D(List<Vector3> pointsInWorld)
        {
            _points = new List<Point>();
            _startPointLineDict = new Dictionary<Point, Line3D>();
            foreach (Vector3 pointInWorld in pointsInWorld)
            {
                _points.Add(new Point(pointInWorld));
            }
        }

        /// <summary>
        /// Constructor which initializes path from given points and sets all the path properties.
        /// </summary>
        /// <param name="pointsInWorld">List of points</param>
        /// <param name="_PointSize">Point size</param>
        /// <param name="_PointColor">Point color</param>
        /// <param name="_LineThickness">Line thickness</param>
        /// <param name="_LineColor">Line color</param>
        public Path3D(List<Vector3> pointsInWorld, float _PointSize, Color _PointColor, float _LineThickness, Color _LineColor)
        {
            _points = new List<Point>();
            _startPointLineDict = new Dictionary<Point, Line3D>();
            PointSize = _PointSize;
            PointColor = _PointColor;
            LineThickness = _LineThickness;
            LineColor = _LineColor;
            foreach (Vector3 pointInWorld in pointsInWorld)
            {
                _points.Add(new Point(pointInWorld, _PointSize, _PointColor));
            }
        }

        /// <summary>
        /// Adds a single point to existing path.
        /// </summary>
        /// <param name="point">Point to add</param>
        public void AddPointToPath(Vector3 point)
        {
            _points.Add(new Point(point));
        }

        /// <summary>
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
                    try
                    {
                        _startPointLineDict[_points[i]].Destroy();
                        _startPointLineDict.Remove(_points[i]);
                        _points[i].Destroy();
                        _points.Remove(_points[i]);
                        i--;
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Draws path
        /// </summary>
        public void Draw()
        {
            for (var i = 0; i < _points.Count-1; i++)
            {
                _points[i].Draw();
                if (_points.Count > 2 && !_startPointLineDict.ContainsKey(_points[i]))
                {
                    _startPointLineDict.Add(_points[i], new Line3D(_points[i].Position, _points[i+1].Position, LineThickness, LineColor));
                }
            }
            if (_points.Count > 0)
            {
                _points[_points.Count - 1].Draw();
            }

            foreach (Line3D line in _startPointLineDict.Values)
            {
                line.Draw();
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

            foreach (Line3D line in _startPointLineDict.Values)
            {
                line.Destroy();
            }
            _startPointLineDict.Clear();
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
            foreach (Line3D line in _startPointLineDict.Values)
            {
                line.SetColor(color);
            }
        }

        public void SetLineThickness(float LineThickness)
        {
            foreach (Line3D line in _startPointLineDict.Values)
            {
                line.SetThickness(LineThickness);
            }
        }
    }
}
