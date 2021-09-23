using System.Collections.Generic;
using Labust.Visualization.Primitives;
using Labust.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Labust.Visualization.Primitives;

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


        void Initialize()
        {
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            _visualElements.Clear();
        }

        void Start()
        {
            // These are some examples how to use
            /*
            List<Vector3> path = new List<Vector3> {new Vector3(0, 1, 0), new Vector3(14, 1, 0), new Vector3(25, 1, 0)};
            AddPath(path, "test path");
            AddPoint(new Vector3(0, 2, 10), "test point", 0.5f);
            var sphere = GameObject.Find("Sphere");
            AddTransform(sphere.transform, "test transform");
            AddLine(new Vector3(0, 1, 0), new Vector3(0, 10, 0), "test line");
            */
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
            CreateAndAttachPointGameObject(p, key);
            _visualElements[key].Add(p);
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
            CreateAndAttachPointGameObject(p, key);
            _visualElements[key].Add(p);
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
            CreateAndAttachPointGameObject(p, key);
            _visualElements[key].Add(p);
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
            CreateAndAttachPointGameObject(point, key);
            _visualElements[key].Add((VisualElement) point);
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
            CreateAndAttachPathGameObject(p, key);
            _visualElements[key].Add(p);
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
            CreateAndAttachPathGameObject(p, key);
            _visualElements[key].Add(p);
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
            CreateAndAttachPathGameObject(path, key);
            _visualElements[key].Add((VisualElement) path);
        }

        /// <summary>
        /// Creates Labust.Visualization.Primitives.Transform object for visualizer to draw
        /// from UnityEngine.Transform
        /// </summary>
        /// <param name="transform">Transform </param>
        /// <param name="key"></param>
        public void AddTransform(UnityEngine.Transform transform, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _transform = (VisualElement) new Labust.Visualization.Primitives.Transform(transform);
            CreateAndAttachTransformGameObject(_transform, key);
            _visualElements[key].Add(_transform);
        }

        public Line AddLine(Vector3 startPoint, Vector3 endPoint, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _line3d = (VisualElement) new Line(startPoint, endPoint, lineThickness, lineColor);
            CreateAndAttachLineGameObject(_line3d, key);
            _visualElements[key].Add(_line3d);
            return (Line) _line3d;
        }

        public Line AddLine(Vector3 startPoint, Vector3 endPoint, string key, float thickness)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _line3d = (VisualElement) new Line(startPoint, endPoint, thickness, lineColor);
            CreateAndAttachLineGameObject(_line3d, key);
            _visualElements[key].Add(_line3d);
            return (Line) _line3d;
        }

        public Line AddLine(Vector3 startPoint, Vector3 endPoint, string key, float thickness, Color color)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            VisualElement _line3d = (VisualElement) new Line(startPoint, endPoint, thickness, color);
            CreateAndAttachLineGameObject(_line3d, key);
            _visualElements[key].Add(_line3d);
            return (Line) _line3d;
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

        private void CreateAndAttachPathGameObject(VisualElement p, string name)
        {
            GameObject path = new GameObject(name);
            PathVisualController pc = path.AddComponent(typeof(PathVisualController)) as PathVisualController;
            pc.MyPath = (Path) p;
            path.transform.SetParent(transform);
            pc.MyPath.SetPointParent(path);
        }

        private void CreateAndAttachPointGameObject(VisualElement p, string name)
        {
            GameObject point = new GameObject(name);
            PointVisualController pc = point.AddComponent(typeof(PointVisualController)) as PointVisualController;
            pc.MyPoint = (Point) p;
            point.transform.SetParent(transform);
            pc.MyPoint.SetParent(point);
        }

        private void CreateAndAttachTransformGameObject(VisualElement t, string name)
        {
            GameObject _transform = new GameObject(name);
            _transform.transform.position = ((Labust.Visualization.Primitives.Transform) t).MyTransform.position;
            TransformVisualController tvc = _transform.AddComponent(typeof(TransformVisualController)) as TransformVisualController;
            tvc.MyTransform = (Labust.Visualization.Primitives.Transform) t;
            _transform.transform.SetParent(transform);
            tvc.MyTransform.SetParent(_transform);
        }

        void CreateAndAttachLineGameObject(VisualElement l, string name)
        {
            GameObject _line = new GameObject(name);
            LineVisualController lvc = _line.AddComponent(typeof(LineVisualController)) as LineVisualController;
            lvc.MyLine = (Line) l;
            _line.transform.SetParent(transform);
            lvc.MyLine.SetParent(_line);
        }

        void Update()
        {
            foreach (var list in _visualElements.Values)
            {
                foreach (var visual in list)
                {
                    visual.Draw();
                }
            }
        }
    }
}
