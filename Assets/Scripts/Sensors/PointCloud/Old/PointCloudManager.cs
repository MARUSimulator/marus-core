using UnityEngine;
using System.Collections;
using Unity.Collections;
using Gemini.EMRS.Core;

namespace Labust.Sensors.Core
{
    public class PointCloudManager : MonoBehaviour
    {
        public ComputeShader computeParticle;
        Material _particleMaterial;

        GameObject _cloudObject;
        Mesh _particleMesh;

        string displaymentLayer = "Lidar";
        [Header("Debug")]
        [SerializeField]int _nrOfParticles;

        public void SetupPointCloud(int nrOfParticles)
        {
            _particleMesh = CreateMesh(nrOfParticles);

            _cloudObject = CreatePointCloudObject(displaymentLayer);
            _cloudObject.GetComponent<MeshFilter>().mesh = _particleMesh;
            _cloudObject.GetComponent<Renderer>().material = _particleMaterial;
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

            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();

            return obj;
        }


        public void UpdatePointCloud(NativeArray<Vector3> points){
            _particleMesh.SetVertices(points);
        }

        public void UpdatePointCloud(Vector3[] points)
        {
            _particleMesh.SetVertices(points);
        }

    }
}