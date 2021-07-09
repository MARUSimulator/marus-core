
using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
    public class Transform : DrawGizmo
    {
        UnityEngine.Transform _transform;
        Visualizer _visualizer;
        public Transform(UnityEngine.Transform transform)
        {
            _transform = transform;
            _visualizer = GameObject.FindObjectOfType<Visualizer>();
        }

        public void Draw()
        {
            var origin = _transform.TransformPoint(Vector3.zero);
            var x = _transform.TransformPoint(Vector3.right);
            var y = _transform.TransformPoint(Vector3.up);
            var z = _transform.TransformPoint(Vector3.forward);
            Gizmos.DrawSphere(origin, _visualizer.pointSize);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, x);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, y);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, z);
        }
    }

}
