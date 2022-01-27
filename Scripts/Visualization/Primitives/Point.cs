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
        /// Position of point in space
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Reference to object's transform property
        /// </summary>
        public UnityEngine.Transform _pointTransform;

        /// <summary>
        /// Point size
        /// </summary>
        public float PointSize = 0.7f;

        /// <summary>
        /// Point color
        /// </summary>
        public Color PointColor = Color.white;

        public PrimitiveType PointType = PrimitiveType.Sphere;

        private GameObject sphere = null;

        private GameObject parent;

        private bool destroyed = false;

        /// <summary>
        /// Constructor which initializes point with given position
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        public Point(Vector3 pointInWorld)
        {
            Position = pointInWorld;
            Timestamp = DateTime.UtcNow;
            Lifetime = 0;
        }

        /// <summary>
        /// Constructor which initializes point with given position and size
        /// </summary>
        /// <param name="pointInWorld">Position in space</param>
        /// <param name="pointSize">Point size</param>
        public Point(Vector3 pointInWorld, float pointSize) : this(pointInWorld)
        {
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
        public override void Draw()
        {
            if (destroyed)
            {
                return;
            }

            if (sphere == null)
            {
                sphere = GameObject.CreatePrimitive(PointType);
                sphere.hideFlags = HideFlags.HideInHierarchy;
                sphere.GetComponent<Collider>().enabled = false;
                sphere.transform.localScale = new Vector3(PointSize, PointSize, PointSize);
                sphere.transform.position = Position;
                sphere.GetComponent<Renderer>().material.color = PointColor;
                sphere.layer = LayerMask.NameToLayer("Visualization");
                sphere.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Points");
                Material newMat = new Material(Shader.Find("HDRP/Unlit"));
                sphere.GetComponent<Renderer>().material = newMat;
                sphere.GetComponent<Renderer>().material.color = PointColor;
            }
            if (sphere != null && _pointTransform != null)
            {
                sphere.transform.position = _pointTransform.position;
            }
            if (parent != null && sphere.transform.parent == null)
            {
                sphere.transform.SetParent(parent.transform);
            }
        }

        /// <summary>
        /// Destroy and remove point
        /// </summary>
        public override void Destroy()
        {
            destroyed = true;
            if (sphere == null)
            {
                return;
            }
            if (parent != null && parent.transform.childCount == 1)
            {
                UnityEngine.Object.Destroy(parent);
            }
            else
            {
                UnityEngine.Object.Destroy(sphere.gameObject);
            }
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
            PointColor = color;
            if (sphere == null)
            {
                return;
            }
            sphere.GetComponent<Renderer>().material.color = color;
        }

        public void SetParent(GameObject parent)
        {
            this.parent = parent;
        }
    }
}
