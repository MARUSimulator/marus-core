
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Draw path as by-part linear.
    /// 
    /// If Transform[] is given in the constructor, follow the transform origins of every point in every frame
    /// </summary>
    public class LinearPath : DrawGizmo
    {
        Vector3[] _points;
        UnityEngine.Transform[] _transformPoints;
        Visualizer _visualizer;
        public LinearPath(Vector3[] pointsInWorld)
        {
            _visualizer = GameObject.FindObjectOfType<Visualizer>();
            _points = pointsInWorld;
        }

        public LinearPath(UnityEngine.Transform[] pointsInWorld)
        {
            _visualizer = GameObject.FindObjectOfType<Visualizer>();
            _transformPoints = pointsInWorld;
        }

        public void Draw()
        {
            Vector3[] points = (_points != null) ? _points : _transformPoints.Select(x => x.position).ToArray();

            Handles.SphereHandleCap(0, points[0], Quaternion.identity, _visualizer.pointSize, EventType.Repaint);
            for (var i = 0; i < points.Length-1; i++)
            {
                // Gizmos.DrawLine(_points[i], _points[i+1]);
                Handles.DrawLine(points[i], points[i+1], _visualizer.lineThickness);
                Handles.SphereHandleCap(0, points[i+1], Quaternion.identity, _visualizer.pointSize, EventType.Repaint);
                // Gizmos.DrawSphere(_points[i+1], _visualizer.pointSize);
            }
        }
    }

}
