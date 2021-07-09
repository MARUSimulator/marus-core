
using UnityEngine;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Draw point as sphere
    /// If Transform is given in the constructor, follow the transform origin in every frame
    /// </summary>
    public class Point : DrawGizmo
    {
        Vector3 _point;
        Visualizer _visualizer;
        UnityEngine.Transform _pointTransform;
        public Point(Vector3 pointInWorld)
        {
            _visualizer = GameObject.FindObjectOfType<Visualizer>();
            _point = pointInWorld;
        }

        public Point(UnityEngine.Transform pointInWorld)
        {
            _visualizer = GameObject.FindObjectOfType<Visualizer>();
            _pointTransform = pointInWorld;
        }

        public void Draw()
        {
            var point = (_point != null) ? _point : _pointTransform.position;
            Gizmos.DrawSphere(point, _visualizer.pointSize);
        }
    }

}
