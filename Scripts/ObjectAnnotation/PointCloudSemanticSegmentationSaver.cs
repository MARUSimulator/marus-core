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
using Marus.Sensors;
using Unity.Collections;
using Marus.Pcd;
using System.IO;
using System;

namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// This class serves as configuration place for pointcoud segmentation dataset collection.
    /// Holds properties like directory, save frequency.
    /// </summary>
    [RequireComponent(typeof(RaycastLidar))]
    public class PointCloudSemanticSegmentationSaver : MonoBehaviour
    {
        /// <summary>
        /// Saving frequency in Hz
        /// </summary>
        public float SaveFrequencyHz = 1f;

        /// <summary>
        /// Dataset directory
        /// </summary>
        public string SavePath;

        /// <summary>
        /// Namespace prefix to put in pcd and label filenames.
        /// </summary>
        public string Namespace = "";
        RaycastLidar lidar;
        float _lastSave = 0;
        Vector3 _lastOrientation;
        Vector3 _lastPosition;
        int counter;
        NativeArray<Vector3> transformedPoints;

        public void OnEnable()
        {
            CreateFolders();
            lidar = GetComponent<RaycastLidar>();
            _lastOrientation = lidar.transform.eulerAngles;
            _lastPosition = lidar.transform.position;
            counter = 0;
            lidar.OnFinishEvent += Save;
        }

        /// <summary>
        /// Transforms points to lidar frame, saves them to PCD file
        /// generates point labels and saves them to file.
        /// Also writes lidar rotation matrix to poses.txt file.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="readings"></param>
        void Save(NativeArray<Vector3> points, NativeArray<LidarReading> readings)
        {
            if (!transformedPoints.IsCreated)
            {
                transformedPoints = new NativeArray<Vector3>(points.Length, Allocator.Persistent);
            }

            if ((Time.time - _lastSave) >= (1 / SaveFrequencyHz))
            {
                var orientation = lidar.transform.eulerAngles - _lastOrientation;
                _lastOrientation = lidar.transform.eulerAngles;
                var orientationMatrix = Matrix4x4.Rotate(Quaternion.Euler(orientation));
                var translation = lidar.transform.position - _lastPosition;
                //poses.txt
                _lastPosition = lidar.transform.position;
                var c = counter.ToString("d6");
                var fileName = Namespace + $"_{c}";
                var pcdFile = Path.Combine(SavePath, "lidar", fileName + ".pcd");
                TransformPoints(points, lidar.transform);
                PCDSaver.WriteToPcdFile(pcdFile, transformedPoints, "binary");
                WriteLabels(fileName, readings);
                WritePose(orientationMatrix, translation);
                _lastSave = Time.time;
                counter++;
            }
        }

        void WritePose(Matrix4x4 matrix, Vector3 translation)
        {
            using (StreamWriter sw = File.AppendText(Path.Combine(SavePath, "poses.txt")))
            {
                sw.WriteLine($"{matrix[0,0]} {matrix[0,1]} {matrix[0,2]} {translation.x} {matrix[1,0]} {matrix[1,1]} {matrix[1,2]} {translation.y} {matrix[2,0]} {matrix[2,1]} {matrix[2,2]} {translation.z}");
            }
        }

        void WriteLabels(string fileName, NativeArray<LidarReading> readings)
        {
            var filePath = Path.Combine(SavePath, "labels", fileName + ".label");
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                foreach (var r in readings)
                {
                    var ClassId = 0;
                    var InstanceId = 0;
                    var hit = r.hit;
                    if (hit.collider != null)
                    {
                        var classDef = hit.collider.gameObject.GetComponentInParent<PointCloudClassDefinition>();
                        if (classDef == null)
                        {
                            // treat unlabeled objects as same instance and same class (other)
                            ClassId = 0;
                            InstanceId = 0;
                        }
                        else
                        {
                            ClassId = classDef.Index;
                            InstanceId = hit.collider.gameObject.GetComponentInParent<AnnotationObjectInstance>().InstanceId;
                        }
                    }
                    else
                    {
                        ClassId = 0;
                        InstanceId = 0;
                    }
                    var classId = BitConverter.GetBytes((ushort) ClassId);
                    var instId = BitConverter.GetBytes((ushort) InstanceId);
                    stream.Write(classId, 0, classId.Length);
                    stream.Write(instId, 0, instId.Length);
                }
            }
        }

        void TransformPoints(NativeArray<Vector3> points, Transform lidar)
        {
            for (int i = 0; i<points.Length; i++)
            {
                transformedPoints[i] = lidar.InverseTransformPoint(points[i]);
            }
        }

        void CreateFolders()
        {
            Directory.CreateDirectory(SavePath);
            Directory.CreateDirectory(Path.Combine(SavePath, "labels"));
            Directory.CreateDirectory(Path.Combine(SavePath, "lidar"));
        }

        void OnDestroy()
        {
            transformedPoints.Dispose();
        }
    }
}
