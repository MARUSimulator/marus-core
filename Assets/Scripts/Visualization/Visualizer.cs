using System.Collections.Generic;
using Labust.Visualization.Primitives;
using UnityEngine;

namespace Labust.Visualization
{
    public class Visualizer : MonoBehaviour
    {
        
        public Color pointColor = Color.blue;
        public float lineThickness = 5f;

        [Range(0, 1)]
        public float pointSize = 0.1f;
        private static Visualizer _instance;
        private List<DrawGizmo> _gizmos;

        void Start()
        {
            // TEST DRAW FUNCTIONALITY
            AddPath(new Vector3[] {new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(2, 0, 0) });

            var boat = GameObject.Find("Boat1");
            AddTransform(boat.transform);

            AddPoint(new Vector3(2, 2, 2));
            AddPoint(new Vector3(2, 2, 3));
            AddPath(new Vector3[] {new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(2, 0, 0) });
        }
        
        public static Visualizer Instance 
        {
            get
            {
                if (_instance == null)
                {
                    var obj = GameObject.FindObjectOfType<Visualizer>();
                    if (obj != null)
                    {
                        _instance = obj;
                        return _instance;
                    }
                    GameObject newInstance = new GameObject();
                    newInstance.name = nameof(Visualizer);
                    DontDestroyOnLoad(newInstance);
                    _instance = newInstance.AddComponent<Visualizer>();
                    return _instance;
                }
                return _instance;
            }
        }

        private Visualizer() : base()
        {
            _gizmos = new List<DrawGizmo>();
        }


        public void AddPoint(Vector3 pointInWorld)
        {
            _gizmos.Add(new Point(pointInWorld));
        }

        public void AddPath(Vector3[] pointsInWorld)
        {
            _gizmos.Add(new LinearPath(pointsInWorld));
        }

        public void AddTransform(UnityEngine.Transform transform)
        {
            _gizmos.Add(new Labust.Visualization.Primitives.Transform(transform));
        }


        void OnDrawGizmos()
        {
            for (var i = 0; i < _gizmos.Count; i++)
            {
                _gizmos[i].Draw();
            }
        }
    }
}