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

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Marus.Visualization.Primitives
{
    /// <summary>
    /// Draw path as by-part linear with points connected with a cylinder 3D primitive object.
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
        public Color LineColor = Color.red;
        /// <summary>
        /// Point size
        /// </summary>
        public float PointSize = 0.7f;
        /// <summary>
        /// Point color
        /// </summary>
        public Color PointColor = Color.white;

        /// <summary>
        /// List containing all path points
        /// </summary>
        private List<Point> _points;

        /// <summary>
        /// Helper dictionary which references Line object by it's starting point
        /// </summary>
        private Dictionary<Point, Line> _startPointLineDict;

        public UnityEvent OnDestroyPath;

        private GameObject parent;

        private bool destroyed = false;

        /// <summary>
        /// Empty constructor which initializes empty _points list and empty 
        /// _startPointLineDict dictionary
        /// </summary>
        public Path()
        {
            _points = new List<Point>();
            _startPointLineDict = new Dictionary<Point, Line>();
            if (OnDestroyPath == null)
            {
                OnDestroyPath = new UnityEvent();
            }
            Timestamp = DateTime.UtcNow;
            Lifetime = 0;

        }

        /// <summary>
        /// Constructor which initializes empty _points list and empty
        /// _startPointLineDict dictionary and sets line color and line thickness.
        /// </summary>
        public Path(float LineThickness, Color lc) : this()
        {
            this.LineThickness = LineThickness;
            LineColor = lc;
        }

        /// <summary>
        /// Constructor which initializes path from list of points.
        /// </summary>
        /// <param name="pointsInWorld">List of points to create a path from.</param>
        public Path(List<Vector3> pointsInWorld) : this()
        {
            foreach (Vector3 pointInWorld in pointsInWorld)
            {
                _points.Add(new Point(pointInWorld));
            }
            for (int i = 0; i < _points.Count - 1; i++)
            {
                if (_points.Count >= 2 && !_startPointLineDict.ContainsKey(_points[i]))
                {
                    _startPointLineDict.Add(_points[i], new Line(_points[i].Position, _points[i+1].Position, LineThickness, LineColor));
                }
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
        public Path(List<Vector3> pointsInWorld, float _PointSize, Color _PointColor, float _LineThickness, Color _LineColor) : this()
        {
            PointSize = _PointSize;
            PointColor = _PointColor;
            LineThickness = _LineThickness;
            LineColor = _LineColor;
            foreach (Vector3 pointInWorld in pointsInWorld)
            {
                _points.Add(new Point(pointInWorld, _PointSize, _PointColor));
            }

            for (int i = 0; i < _points.Count - 1; i++)
            {
                if (_points.Count >= 2 && !_startPointLineDict.ContainsKey(_points[i]))
                {
                    _startPointLineDict.Add(_points[i], new Line(_points[i].Position, _points[i+1].Position, LineThickness, LineColor));
                }
            }
        }

        /// <summary>
        /// Adds a single point to existing path.
        /// </summary>
        /// <param name="point">Point to add</param>
        public void AddPointToPath(Vector3 point)
        {
            Point p = new Point(point);
            _points.Add(p);
            if (_points.Count >= 2 && !_startPointLineDict.ContainsKey(p))
            {
                _startPointLineDict.Add(_points[_points.Count - 2], new Line(_points[_points.Count - 2].Position, p.Position, LineThickness, LineColor));
            }
        }

        /// <summary>
        /// Adds a single point to existing path with given size and color.
        /// </summary>
        /// <param name="point">Point to add</param>
        /// <param name="PointSize">Point size</param>
        /// <param name="PointColor">Point color</param>
        public void AddPointToPath(Vector3 point, float PointSize, Color PointColor)
        {
            Point p = new Point(point, PointSize, PointColor);
            _points.Add(p);
            if (_points.Count >= 2 && !_startPointLineDict.ContainsKey(p))
            {
                _startPointLineDict.Add(_points[_points.Count - 2], new Line(_points[_points.Count - 2].Position, p.Position, LineThickness, LineColor));
            }
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
        public override void Draw()
        {
            for (var i = 0; i < _points.Count-1; i++)
            {
                _points[i].Draw();
            }
            if (_points.Count > 0)
            {
                _points[_points.Count - 1].Draw();
            }

            foreach (Line line in _startPointLineDict.Values)
            {
                line.Draw();
            }
        }

        /// <summary>
        /// Destroys path object
        /// </summary>
        public override void Destroy()
        {
            if (destroyed) return;

            if (parent != null && parent.transform.childCount == (_points.Count*2 -1))
            {
                UnityEngine.Object.Destroy(parent);
            }

            foreach (Point p in _points)
            {
                p.Destroy();
            }
            _points.Clear();

            foreach (Line line in _startPointLineDict.Values)
            {
                line.Destroy();
            }
            _startPointLineDict.Clear();

            OnDestroyPath.Invoke();
            destroyed = true;
        }

        public void SetPointSize(float size)
        {
            this.PointSize = size;
            foreach (Point p in _points)
            {
                p.SetSize(size);
            }
        }

        public void SetPointColor(Color color)
        {
            this.PointColor = color;
            foreach (Point p in _points)
            {
                p.SetColor(color);
            }
        }

        public void SetLineColor(Color color)
        {
            this.LineColor = color;
            foreach (Line line in _startPointLineDict.Values)
            {
                line.SetColor(color);
            }
        }

        public void SetLineThickness(float LineThickness)
        {
            this.LineThickness = LineThickness;
            foreach (Line line in _startPointLineDict.Values)
            {
                line.SetThickness(LineThickness);
            }
        }

        public void SetPointParent(GameObject parent)
        {
            this.parent = parent;
            if (_points != null)
            {
                foreach (Point p in _points)
                {
                    p.SetParent(parent);
                }
            }
            if (_startPointLineDict != null)
            {
                foreach (Line l in _startPointLineDict.Values)
                {
                    l.SetParent(parent);
                }
            }
        }

        public Vector3 GetPathPosition()
        {
            if (_points.Count > 0)
            {
                return _points[0].Position;
            }
            return Vector3.zero;
        }

        public Vector3 GetPathDimension()
        {
            if (_points.Count > 0)
            {
                Vector3 min, max;
                min = _points[0].Position;
                max = _points[0].Position;
                foreach (Point point in _points)
                {
                    min = Vector3.Min(min,point.Position);
                    max = Vector3.Max(max,point.Position);
                }
                return max-min;
            }
            return Vector3.zero;
        }

        public GameObject GetPathGameObject()
        {
            return parent;
        }
    }
}
