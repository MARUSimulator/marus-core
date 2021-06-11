using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Labust.Sensors.Core
{
    public class PointCloudVisualizer : MonoBehaviour
    {
        public int population;
        public float range;

        public Material material;
        public ComputeShader compute;
        public Transform pusher;

        private ComputeBuffer meshPropertiesBuffer;
        private ComputeBuffer argsBuffer;

        private Mesh mesh;
        //public Mesh ExternalMesh;
        private Bounds bounds;

        // Mesh Properties struct to be read from the GPU.
        // Size() is a convenience funciton which returns the stride of the struct.
        private struct MeshProperties
        {
            public Matrix4x4 mat;
            public Vector4 color;

            public static int Size()
            {
                return
                    sizeof(float) * 4 * 4 + // matrix;
                    sizeof(float) * 4;      // color;
            }
        }
        public float particleSize = 1f;

        private void Start()
        {
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Mesh mesh = sphere.GetComponent<MeshFilter>().mesh;
            //Mesh mesh = ExternalMesh;

            Mesh mesh = PointMesh.Quad(particleSize);
            this.mesh = mesh;

            // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
            bounds = new Bounds(transform.position, Vector3.one * (range + 1));
            InitializeBuffers();
        }

        //public Transform cameraPosition;
        private void Update()
        {
            int kernel = compute.FindKernel("CSMain");
            compute.SetVector("_PusherPosition", pusher.position);
            // We used to just be able to use `population` here, but it looks like a Unity update imposed a thread limit (65535) on my device.
            // This is probably for the best, but we have to do some more calculation.  Divide population by numthreads.x in the compute shader.
            compute.Dispatch(kernel, Mathf.CeilToInt(population / 1024f), 1, 1);
            //bounds.center = cameraPosition.position;
            bounds = new Bounds(transform.position, Vector3.one * (range + 1));
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
            //Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer, 0, null, ShadowCastingMode.On, true, 0);// Camera camera = null, Rendering.LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes, LightProbeProxyVolume lightProbeProxyVolume = null);
        }

        private void InitializeBuffers()
        {
            int kernel = compute.FindKernel("CSMain");

            // Argument buffer used by DrawMeshInstancedIndirect.
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            // Arguments for drawing mesh.
            // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)population;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);

            // Initialize buffer with the given population.
            MeshProperties[] properties = new MeshProperties[population];
            for (int i = 0; i < population; i++)
            {
                MeshProperties props = new MeshProperties();
                Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
                Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                Vector3 scale = Vector3.one;

                props.mat = Matrix4x4.TRS(position, rotation, scale);
                props.color = Color.Lerp(Color.red, Color.blue, Random.value);

                properties[i] = props;
            }

            meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
            meshPropertiesBuffer.SetData(properties);
            compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
            material.SetBuffer("_Properties", meshPropertiesBuffer);
        }

        // https://www.youtube.com/watch?v=7gRrujBjsFo&t=1973s
        private void OnValidate()
        {
            InitializeBuffers();
        }

        private void OnDisable()
        {
            if (meshPropertiesBuffer != null)
            {
                meshPropertiesBuffer.Release();
            }
            meshPropertiesBuffer = null;

            if (argsBuffer != null)
            {
                argsBuffer.Release();
            }
            argsBuffer = null;
        }
    }
}
