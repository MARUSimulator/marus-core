using UnityEngine;
using System;

namespace Labust.Visualization.Primitives
{
    /// <summary>
    /// Draw point as primitive sphere object
    /// If Transform is given in the constructor, follow the transform origin in every frame
    /// </summary>
    public class Point : VisualElement
    {
        /// <summary>
        /// Positionof point in space
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Reference to object's transform property
        /// </summary>
        public UnityEngine.Transform _pointTransform;

        /// <summary>
        /// Timestamp when point was created
        /// </summary>
        public DateTime Timestamp;

        /// <summary>
        /// Point size
        /// </summary>
        public float PointSize = 0.1f;

        /// <summary>
        /// Point color
        /// </summary>
        public Color PointColor = Color.blue;

        private GameObject sphere = null;

        /// <summary>
        /// Constructor which initializes point with given position
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        public Point(Vector3 pointInWorld)
        {
            Position = pointInWorld;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor which initializes point with given position and size
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="pointSize">Point size</param>
        public Point(Vector3 pointInWorld, float pointSize)
        {
            Position = pointInWorld;
            Timestamp = DateTime.UtcNow;
            PointSize = pointSize;
        }

        /// <summary>
        /// Constructor which initializes point with given position, size and color
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="pointSize">Point size</param>
        /// <param name="pointColor">Point color</param>
        public Point(Vector3 pointInWorld, float pointSize, Color pointColor)
        {
            Position = pointInWorld;
            Timestamp = DateTime.UtcNow;
            PointSize = pointSize;
            PointColor = pointColor;
        }

        /// <summary>
        /// Constructor which initializes point with given GamObject transform
        /// </summary>
        /// <param name="pointInWorld">GameObject's transform reference</param>
        public Point(UnityEngine.Transform pointInWorld)
        {
            _pointTransform = pointInWorld;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor which initializes point with given GamObject transform and point size
        /// </summary>
        /// <param name="pointInWorld">GameObject's transform reference</param>
        /// <param name="pointSize">Point size</param>
        public Point(UnityEngine.Transform pointInWorld, float pointSize)
        {
            _pointTransform = pointInWorld;
            Timestamp = DateTime.UtcNow;
            PointSize = pointSize;
        }

        /// <summary>
        /// Constructor which initializes point with given GamObject transform, point size and color
        /// </summary>
        /// <param name="pointInWorld">GameObject's transform reference</param>
        /// <param name="pointSize">Point size</param>
        /// <param name="pointColor">Point color</param>
        public Point(UnityEngine.Transform pointInWorld, float pointSize, Color pointColor)
        {
            _pointTransform = pointInWorld;
            Timestamp = DateTime.UtcNow;
            PointSize = pointSize;
            PointColor = pointColor;
        }

        /// <summary>
        /// Draw point
        /// </summary>
        public void Draw()
        {
            if (sphere == null)
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.hideFlags = HideFlags.HideInHierarchy;
                sphere.GetComponent<SphereCollider>().enabled = false;
                sphere.transform.localScale = new Vector3(PointSize, PointSize, PointSize);
                sphere.transform.position = Position;
                sphere.GetComponent<Renderer>().material.color = PointColor;
                sphere.layer = 6;
            }
            if (sphere != null && _pointTransform != null)
            {
                sphere.transform.position = _pointTransform.position;
            }
        }

        /// <summary>
        /// Destroy and remove point
        /// </summary>
        public void Destroy()
        {
            if (sphere == null)
            {
                return;
            }
            UnityEngine.Object.Destroy(sphere.gameObject);
            sphere = null;
        }

        public void SetSize(float size)
        {
            if (sphere == null)
            {
                return;
            }
            sphere.transform.localScale = new Vector3(size, size, size);
        }

        public void SetColor(Color color)
        {
            if (sphere == null)
            {
                return;
            }
            sphere.GetComponent<Renderer>().material.color = color;
        }
    }
}
