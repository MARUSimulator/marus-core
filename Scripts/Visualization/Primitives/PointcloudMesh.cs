using UnityEngine;
using System;
using Marus.Utils;
using UnityEngine.Rendering;

namespace Marus.Visualization.Primitives
{
    /// <summary>
    /// Draw pointcloud as mesh
    /// </summary>
    public class PointcloudMesh : VisualElement
    {
        /// <summary>
        /// Object containing points, normals and colors
        /// </summary>
        public PointCloud PointCloud;

        /// <summary>
        /// Position of pointcloud in space
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Name for the pointcloud gameObject to show in hierarchy
        /// </summary>
        #nullable enable
        public string? Name = null;
        #nullable disable

        private GameObject pc = null;

        private GameObject parent;

        private bool destroyed = false;

        public Mesh myMesh;
        private MeshFilter meshFilter;

		public PointcloudMesh()
		{
			Timestamp = DateTime.UtcNow;
            myMesh = new Mesh();
		}

        public PointcloudMesh(PointCloud pc) : this()
		{
			PointCloud = pc;
		}

        public PointcloudMesh(PointCloud pc, string name) : this(pc)
		{
			Name = name;
		}

        /// <summary>
        /// Draw pointcloud
        /// </summary>
        public override void Draw()
        {
            if (destroyed)
            {
                return;
            }

            if (pc == null)
            {
                pc = new GameObject(Name != null ? Name : "PointCloud object");
                pc.layer = LayerMask.NameToLayer("Visualization");

                pc.AddComponent<MeshFilter>().mesh = myMesh;
                var renderer = pc.AddComponent<MeshRenderer>();
                myMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
                if (PointCloud.Points.Length > 65535)
                {
                    myMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }


                renderer.material = new Material (Shader.Find("Point Cloud/Point"));
                var colored = PointCloud.Colors != null;

                int[] indices = new int[PointCloud.Points.Length];

                for(int i=0;i<PointCloud.Points.Length;++i)
                {
                    indices[i] = i;
                }

                myMesh.vertices = PointCloud.Points;
                if (PointCloud.Normals != null)
                {
                    myMesh.normals = PointCloud.Normals;
                }

                myMesh.colors = PointCloud.Colors;
                myMesh.SetIndices(indices, MeshTopology.Points, 0);
                myMesh.UploadMeshData(false);
            }

            if (parent != null && pc.transform.parent == null)
            {
                pc.transform.SetParent(parent.transform);
            }
        }

        public void SetNewVertices(Vector3[] vertices)
        {
            myMesh.vertices = vertices;
        }

        /// <summary>
        /// Destroy and remove point
        /// </summary>
        public override void Destroy()
        {
            destroyed = true;
            if (PointCloud == null)
            {
                return;
            }
            UnityEngine.Object.Destroy(pc.gameObject);
            PointCloud = null;
            pc = null;
        }
        public void SetParent(GameObject parent)
        {
            this.parent = parent;
        }
    }
}
