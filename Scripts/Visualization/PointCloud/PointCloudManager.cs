using UnityEngine;
using System.Collections;
using Unity.Collections;
using Gemini.EMRS.Core;
using Labust.Sensors.Core;

namespace Labust.Visualization
{
    public class PointCloudManager : MonoBehaviour
    {
        public ComputeShader computeParticle;
        public Material particleMaterial;

        GameObject _cloudObject;
        Mesh _particleMesh;

        string displaymentLayer = "Lidar";
        [Header("Debug")]
        [SerializeField]int _nrOfParticles;

        public void SetupPointCloud(int nrOfParticles)
        {
            _particleMesh = CreateMesh(nrOfParticles);

            _cloudObject = CreatePointCloudObject(displaymentLayer);
        }

        private Mesh CreateMesh(int nrOfParticles)
        {
            _nrOfParticles = nrOfParticles;

            int[] indices = new int[_nrOfParticles];
            indices = ArrayAllocator(_nrOfParticles, computeParticle);

            Mesh mesh = new Mesh { vertices = new Vector3[_nrOfParticles] };
            mesh.SetIndices(indices, MeshTopology.Points, 0);

            return mesh;
        }

        private static int[] ArrayAllocator(int length, ComputeShader shader)
        {
            string kernelName = "QuickArrayAllocation";

            ComputeBufferDataExtractor<int> array = new ComputeBufferDataExtractor<int>(length, sizeof(int), "array");
            array.SetBuffer(shader, kernelName);
            shader.SetInt("arrayLength", length);

            return array.SynchUpdate(shader, kernelName);
        }

        private GameObject CreatePointCloudObject(string displaymentLayer)
        {
            GameObject obj = new GameObject();

            this.displaymentLayer = displaymentLayer;
            obj.layer = LayerMask.NameToLayer(this.displaymentLayer);

            obj.name = "PointCloud";

            obj.transform.SetParent(transform);
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.transform.localPosition = new Vector3(0, 0, 0);

            var mesh = obj.AddComponent<MeshFilter>();
            mesh.mesh = _particleMesh;
            var renderer = obj.AddComponent<MeshRenderer>();
            renderer.material = particleMaterial;
            return obj;
        }


        public void UpdatePointCloud(NativeArray<Vector3> points){
            _particleMesh.SetVertices(points);
            _particleMesh.RecalculateBounds();
        }

        public void UpdatePointCloud(Vector3[] points)
        {
            _particleMesh.SetVertices(points);
            _particleMesh.RecalculateBounds();
        }

        public static PointCloudManager CreatePointCloud(string name, int numPoints, Material particleMaterial, ComputeShader computeShader)
        {
            GameObject pointCloud = new GameObject(name + "_PointCloud");
            pointCloud.transform.position = Vector3.zero;
            pointCloud.transform.rotation = Quaternion.identity;
            var pointCloudManager = pointCloud.AddComponent<PointCloudManager>();
            pointCloudManager.particleMaterial = particleMaterial;
            pointCloudManager.computeParticle = computeShader ?? FindComputeShader("PointCloudCS");
            pointCloudManager.SetupPointCloud(numPoints);
            return pointCloudManager;
        }

        private static ComputeShader FindComputeShader(string shaderName)
        {
            ComputeShader[] compShaders = (ComputeShader[])Resources.FindObjectsOfTypeAll(typeof(ComputeShader));
            for (int i = 0; i < compShaders.Length; i++)
            {
                if (compShaders[i].name == shaderName)
                {
                    return compShaders[i];
                }
            }
            throw new UnityException($"Shader {shaderName} not found in the project");
        }

    }
}