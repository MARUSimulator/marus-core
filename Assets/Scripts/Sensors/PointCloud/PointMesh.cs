using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labust.Sensors.Core
{
    public static class PointMesh
    {
        public static Mesh Quad(float size = 1f)
        {
            // Create a quad mesh.
            var mesh = new Mesh();

            float w = size * .5f;
            float h = size * .5f;
            var vertices = new Vector3[4] 
            {
                new Vector3(-w, -h, 0),
                new Vector3(w, -h, 0),
                new Vector3(-w, h, 0),
                new Vector3(w, h, 0)
            };

            var tris = new int[6] {
                // lower left tri.
                0, 2, 1,
                // lower right tri
                2, 3, 1
            };

            var normals = new Vector3[4] {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
            };

            var uv = new Vector2[4] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };

            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.normals = normals;
            mesh.uv = uv;

            return mesh;
        }
    }
}
