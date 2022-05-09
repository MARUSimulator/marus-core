// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using Unity.Collections;
using Marus.Sensors.Core;

namespace Marus.Visualization
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
        static ComputeBufferDataExtractor<int> array;

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
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            if (_nrOfParticles > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.MarkDynamic();

            return mesh;
        }

        private static int[] ArrayAllocator(int length, ComputeShader shader)
        {
            string kernelName = "QuickArrayAllocation";

            array = new ComputeBufferDataExtractor<int>(length, sizeof(int), "array");
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

        public static PointCloudManager CreatePointCloud(GameObject parentObj, string name, int numPoints, Material particleMaterial, ComputeShader computeShader)
        {
            GameObject pointCloud = new GameObject(name + "_PointCloud");
            pointCloud.transform.parent = parentObj.transform;
            pointCloud.transform.localPosition = Vector3.zero;
            pointCloud.transform.localRotation = Quaternion.identity;
            var pointCloudManager = pointCloud.AddComponent<PointCloudManager>();
            pointCloudManager.particleMaterial = particleMaterial;
            pointCloudManager.computeParticle = computeShader ?? FindComputeShader("PointCloudCS");
            pointCloudManager.SetupPointCloud(numPoints);
            return pointCloudManager;
        }

        private static ComputeShader FindComputeShader(string shaderName)
        {
            ComputeShader cs = (ComputeShader)Resources.Load(shaderName);
            if (cs != null)
            {
                return cs;
            }
            throw new UnityException($"Shader {shaderName} not found in the project");
        }
    }
}
