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
using Labust.Visualization.Primitives;
using Labust.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Labust.Visualization
{
    /// <summary>
    /// This is a singleton class used for drawing points and paths for visualizatoin purposes.
    /// You can add points and paths with a string key tag for easy selective destroying of visualization objects.
    /// Note: Visualizer object is not needed for drawing VisualElement objects.
    /// </summary>
    public class Visualizer : Singleton<Visualizer>
    {
        /// <summary>
        /// Point color default if not provided any other way
        /// </summary>
        private Color pointColor = Color.white;

        /// <summary>
        /// Default point size
        /// </summary>
        private float pointSize = 0.7f;

        /// <summary>
        /// Default line thickness
        /// </summary>
        private float lineThickness = 0.05f;

        /// <summary>
        /// Default line color
        /// </summary>
        private Color lineColor = Color.red;

        /// <summary>
        /// Dictionary used for storing visual elements by string key
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<VisualElement>> _visualElements = new Dictionary<string, List<VisualElement>>();

        [System.Flags] public enum FilterValues { Points=1, Lines=2, Transforms=4 }
        [EnumFlags(typeof(FilterValues))]
        public FilterValues DrawFilter;

        protected override void Initialize()
        {
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            if (string.IsNullOrEmpty(oldScene.name)) return;
            ClearAll();
            _visualElements.Clear();
        }

        void Start()
        {
        }

        private bool AddVisual(string key, VisualElement visual)
        {
            bool exists = false;
            foreach (var v in _visualElements[key])
            {
                if (visual.Id != null && v.Id == visual.Id)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                _visualElements[key].Add(visual);
                return true;
            }
            else
            {
                Debug.Log($"Visual element with id: {visual.Id} already exists!");
            }
            return false;
        }

        /// <summary>
        /// Adds single point for visualizer to draw
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="key">String key tag</param>
        public Point AddPoint(Vector3 pointInWorld, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement p = (VisualElement) new Point(pointInWorld);
            bool created = AddVisual(key, p);
            if (created)
            {
                CreateAndAttachPointGameObject(p, key);
            }
            return (Point) p;
        }

        /// <summary>
        /// Adds single point for visualizer to draw and sets it's size
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="key">String key tag</param>
        /// <param name="pointSize">Point size</param>
        public Point AddPoint(Vector3 pointInWorld, string key, float pointSize)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement p = (VisualElement) new Point(pointInWorld, pointSize);
            bool created = AddVisual(key, p);
            if (created)
            {
                CreateAndAttachPointGameObject(p, key);
            }
            return (Point) p;
        }

        /// <summary>
        /// Adds single point for visualizer to draw and sets it's size and color
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="key">String key tag</param>
        /// <param name="pointSize">Point size</param>
        /// <param name="pointColor">Point color</param>
        public Point AddPoint(Vector3 pointInWorld, string key, float pointSize, Color pointColor)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement p = (VisualElement) new Point(pointInWorld, pointSize, pointColor);
            bool created = AddVisual(key, p);
            if (created)
            {
                CreateAndAttachPointGameObject(p, key);
            }
            return (Point) p;
        }

        /// Adds Point object for visualizer to draw
        /// </summary>
        /// <param name="point">Point object to add</param>
        public void AddPoint(Point point, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            bool created = AddVisual(key, (VisualElement) point);
            if (created)
            {
                CreateAndAttachPointGameObject(point, key);
            }
        }

        /// <summary>
        /// Creates Path object for visualizer to draw from given positions list
        /// </summary>
        /// <param name="pointsInWorld">List of positions for path initialization</param>
        /// <param name="key">String key tag</param>
        public Path AddPath(List<Vector3> pointsInWorld, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement p = (VisualElement) new Path(pointsInWorld, pointSize, pointColor, lineThickness, lineColor);
            bool created = AddVisual(key, p);
            if (created)
            {
                CreateAndAttachPathGameObject(p, key);
            }
            return (Path) p;
        }

        /// <summary>
        /// Creates Path object for visualizer to draw from given positions list from given positions list and sets point color
        /// </summary>
        /// <param name="pointsInWorld">List of positions for path initialization</param>
        /// <param name="key">String key tag</param>
        /// <param name="_pointColor">Point color</param>
        public Path AddPath(List<Vector3> pointsInWorld, string key, Color _pointColor)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement p = (VisualElement) new Path(pointsInWorld, pointSize, _pointColor, lineThickness, lineColor);
            bool created = AddVisual(key, p);
            if (created)
            {
                CreateAndAttachPathGameObject(p, key);
            }
            return (Path) p;
        }

        /// <summary>
        /// Adds Path object for visualizer to draw
        /// </summary>
        /// <param name="path">Path object</param>
        /// <param name="key">String key tag</param>
        public void AddPath(Path path, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            bool created = AddVisual(key, (VisualElement) path);
            if (created)
            {
                CreateAndAttachPathGameObject(path, key);
            }
        }

        /// <summary>
        /// Creates Labust.Visualization.Primitives.Transform object for visualizer to draw
        /// from UnityEngine.Transform
        /// </summary>
        /// <param name="transform">Transform </param>
        /// <param name="key"></param>
        public Primitives.Transform AddTransform(UnityEngine.Transform transform, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            var _transform = new Primitives.Transform(transform);
            bool created = AddVisual(key, _transform);
            if (created)
            {
                AttachTransformGameObject(_transform);
            }
            return _transform;
        }

        public Line AddLine(Vector3 startPoint, Vector3 endPoint, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _line3d = (VisualElement) new Line(startPoint, endPoint, lineThickness, lineColor);
            bool created = AddVisual(key, _line3d);
            if (created)
            {
                CreateAndAttachLineGameObject(_line3d, key);
            }
            return (Line) _line3d;
        }

        public Line AddLine(Vector3 startPoint, Vector3 endPoint, string key, float thickness)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _line3d = (VisualElement) new Line(startPoint, endPoint, thickness, lineColor);
            bool created = AddVisual(key, _line3d);
            if (created)
            {
                CreateAndAttachLineGameObject(_line3d, key);
            }
            return (Line) _line3d;
        }

        public Line AddLine(Vector3 startPoint, Vector3 endPoint, string key, float thickness, Color color)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _line3d = (VisualElement) new Line(startPoint, endPoint, thickness, color);
            bool created = AddVisual(key, _line3d);
            if (created)
            {
                CreateAndAttachLineGameObject(_line3d, key);
            }
            return (Line) _line3d;
        }

        public Arrow AddArrow(Vector3 startPoint, Vector3 endPoint, string key, float radius, Color color)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _arrow = (VisualElement) new Arrow(startPoint, endPoint, radius, color);
            bool created = AddVisual(key, _arrow);
            if (created)
            {
                CreateAndAttachArrowGameObject(_arrow, key);
            }
            return (Arrow) _arrow;
        }

        public Arrow AddArrow(Vector3 startPoint, Vector3 endPoint, string key, float radius, Color color, float headRadius, Color headColor)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _arrow = (VisualElement) new Arrow(startPoint, endPoint, radius, color, headRadius, headColor);
            bool created = AddVisual(key, _arrow);
            if (created)
            {
                CreateAndAttachArrowGameObject(_arrow, key);
            }
            return (Arrow) _arrow;
        }

        public void AddArrow(Arrow arrow, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            bool created = AddVisual(key, (VisualElement) arrow);
            if (created)
            {
                CreateAndAttachArrowGameObject(arrow, key);
            }
        }

        /// <summary>
        /// Removes all objects stored under given key.
        /// </summary>
        /// <param name="key">String key tag</param>
        public void FlushKey(string key)
        {
            foreach (VisualElement visual in _visualElements[key])
            {
                visual.Destroy();
            }
            _visualElements.Remove(key);
        }

        /// <summary>
        /// Removes visual element by it's Id
        /// </summary>
        /// <param name="string">String id of visual element</param>
        public void RemoveById(string id)
        {
            var markForDeletion = new Tuple<string, VisualElement>("", null);
            foreach (var kvp in _visualElements)
            {
                foreach (var visual in kvp.Value)
                {
                    if (visual.Id != null && visual.Id == id)
                    {
                        markForDeletion = new Tuple<string, VisualElement>(kvp.Key, visual);
                        break;
                    }
                }
            }
            if (markForDeletion.Item2 != null)
            {
                _visualElements[markForDeletion.Item1].Remove(markForDeletion.Item2);
                markForDeletion.Item2.Destroy();
            }
        }

        /// <summary>
        /// Removes all objects stored under given key.
        /// </summary>
        /// <param name="key">String key tag</param>
        public void ClearAll()
        {
            foreach (string key in _visualElements.Keys)
            {
                FlushKey(key);
            }
        }

        private void CreateAndAttachPathGameObject(VisualElement p, string name)
        {
            GameObject path = new GameObject($"{name}_path_{p.Id}");
            PathVisualController pc = path.AddComponent(typeof(PathVisualController)) as PathVisualController;
            pc.MyPath = (Path) p;
            path.transform.SetParent(transform);
            pc.MyPath.SetPointParent(path);
        }

        private void CreateAndAttachPointGameObject(VisualElement p, string name)
        {
            GameObject point = new GameObject($"{name}_point_{p.Id}");
            PointVisualController pc = point.AddComponent(typeof(PointVisualController)) as PointVisualController;
            pc.MyPoint = (Point) p;
            point.transform.SetParent(transform);
            pc.MyPoint.SetParent(point);
        }

        private void AttachTransformGameObject(Primitives.Transform t)
        {
            var tvc = t.MyTransform.gameObject.AddComponent<TransformVisualController>();
            tvc.MyTransform = t;
        }

        void CreateAndAttachLineGameObject(VisualElement l, string name)
        {
            GameObject _line = new GameObject($"{name}_line_{l.Id}");
            LineVisualController lvc = _line.AddComponent(typeof(LineVisualController)) as LineVisualController;
            lvc.MyLine = (Line) l;
            _line.transform.SetParent(transform);
            lvc.MyLine.SetParent(_line);
        }

        void CreateAndAttachArrowGameObject(VisualElement a, string name)
        {
            GameObject _arrow = new GameObject($"{name}_arrow_{a.Id}");
            ArrowVisualController avc = _arrow.AddComponent(typeof(ArrowVisualController)) as ArrowVisualController;
            avc.MyArrow = (Arrow) a;
            _arrow.transform.SetParent(transform);
            avc.MyArrow.SetParent(_arrow);
        }

        void Update()
        {
            var selected = EnumFlagsAttribute.ReturnSelectedElements((int)DrawFilter, typeof(FilterValues));
            var markForDeletion = new List<Tuple<string, VisualElement>>();
            foreach (var kvp in _visualElements)
            {
                if (selected.Contains(kvp.Key))
                    continue;
                foreach (var visual in kvp.Value)
                {
                    if (visual.Lifetime != 0 && (DateTime.UtcNow.Subtract(visual.Timestamp).TotalSeconds > visual.Lifetime))
                    {
                        markForDeletion.Add(new Tuple<string, VisualElement>(kvp.Key, visual));
                    }
                    else
                    {
                        visual.Draw();
                    }
                }
            }
            foreach (var kvp in markForDeletion)
            {
                _visualElements[kvp.Item1].Remove(kvp.Item2);
                kvp.Item2.Destroy();
            }
        }
    }
}
