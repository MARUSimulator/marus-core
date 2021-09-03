using System.Collections.Generic;
using Labust.Visualization.Primitives;
using Labust.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Labust.Visualization
{
    /// <summary>
    /// This is a singleton class used for drawing points and paths for visualizatoin purposes.
    /// You can add points and paths with a string key tag for easy selective destroying of visualization objects.
    /// Note: Visualizer object is not needed for drawing VisualElement objects.
    /// </summary>
    public class Visualizer : GenericSingleton<Visualizer>
    {
        /// <summary>
        /// Point color default if not provided any other way
        /// </summary>
        public Color pointColor = Color.yellow;

        /// <summary>
        /// Point size
        /// </summary>
        [Range(0, 1)]
        public float pointSize = 0.1f;

        /// <summary>
        /// Line thickness
        /// </summary>
        [Range(0, 1)]
        public float lineThickness = 0.05f;

        /// <summary>
        /// Line color
        /// </summary>
        public Color lineColor = Color.red;

        /// <summary>
        /// Dictionary used for storing visual elements by string key
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<VisualElement>> _visualElements = new Dictionary<string, List<VisualElement>>();


        void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            _gizmos.Clear();
        }

        void Start()
        {
            // This are some examples how to use

            /*
            var boat = GameObject.Find("Target");
            AddTransform(boat.transform, "test transform");

            AddPoint(new Vector3(0, 2, 10), "test point", 0.5f);

            List<Vector3> path = new List<Vector3> {new Vector3(0, 1, 0), new Vector3(14, 1, 0), new Vector3(25, 1, 0)};
            //AddPath(path, "test path");
            AddPath3D(path, "test path");
            */
        }

        /// <summary>
        /// Adds single point for visualizer to draw
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="key">String key tag</param>
        public void AddPoint(Vector3 pointInWorld, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Point(pointInWorld));
        }

        /// <summary>
        /// Adds single point for visualizer to draw and sets it's size
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="key">String key tag</param>
        /// <param name="pointSize">Point size</param>
        public void AddPoint(Vector3 pointInWorld, string key, float pointSize)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Point(pointInWorld, pointSize));
        }

        /// <summary>
        /// Adds single point for visualizer to draw and sets it's size and color
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="key">String key tag</param>
        /// <param name="pointSize">Point size</param>
        /// <param name="pointColor">Point color</param>
        public void AddPoint(Vector3 pointInWorld, string key, float pointSize, Color pointColor)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Point(pointInWorld, pointSize, pointColor));
        }

        /// <summary>
        /// Creates Path object for visualizer to draw from given positions list
        /// </summary>
        /// <param name="pointsInWorld">List of positions for path initialization</param>
        /// <param name="key">String key tag</param>
        public void AddPath(List<Vector3> pointsInWorld, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Path(pointsInWorld, pointSize, pointColor, lineThickness, lineColor));
        }

        /// <summary>
        /// Creates Path object for visualizer to draw from given positions list and sets point color
        /// </summary>
        /// <param name="pointsInWorld">List of positions for path initialization</param>
        /// <param name="key">String key tag</param>
        /// <param name="_pointColor">Point color</param>
        public void AddPath(List<Vector3> pointsInWorld, string key, Color _pointColor)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Path(pointsInWorld, pointSize, _pointColor, lineThickness, lineColor));
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
            _visualElements[key].Add((VisualElement) path);
        }

        /// <summary>
        /// Creates Path3D object for visualizer to draw from given positions list
        /// </summary>
        /// <param name="pointsInWorld">List of positions for path initialization</param>
        /// <param name="key">String key tag</param>
        public void AddPath3D(List<Vector3> pointsInWorld, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Path3D(pointsInWorld, pointSize, pointColor, lineThickness, lineColor));
        }

        /// <summary>
        /// Creates Path3D object for visualizer to draw from given positions list from given positions list and sets point color
        /// </summary>
        /// <param name="pointsInWorld">List of positions for path initialization</param>
        /// <param name="key">String key tag</param>
        /// <param name="_pointColor">Point color</param>
        public void AddPath3D(List<Vector3> pointsInWorld, string key, Color _pointColor)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
            _visualElements[key].Add((VisualElement) new Path3D(pointsInWorld, pointSize, _pointColor, lineThickness, lineColor));
        }

        /// <summary>
        /// Adds Path3D object for visualizer to draw
        /// </summary>
        /// <param name="path">Path3D object</param>
        /// <param name="key">String key tag</param>
        public void AddPath3D(Path3D path, string key)
        {
            if (!_visualElements.ContainsKey(key))
            {
                _visualElements[key] = new List<VisualElement>();
            }
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
            _visualElements[key].Add((VisualElement) new Labust.Visualization.Primitives.Transform(transform));
        }

        /// <summary>
        /// Removes all objects stored under given key.
        /// </summary>
        /// <param name="key">String key tag</param>
        public void FlushKey(string key)
        {
            foreach (VisualElement gizmo in _visualElements[key])
            {
                gizmo.Destroy();
            }
            _visualElements.Remove(key);
        }

        void Update()
        {
            foreach (var list in _visualElements.Values)
            {
                foreach (var gizmo in list)
                {
                    gizmo.Draw();
                }
            }
        }
    }
}
