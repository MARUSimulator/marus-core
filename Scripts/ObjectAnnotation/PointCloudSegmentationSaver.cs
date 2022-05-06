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
using Marus.Utils;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// This class serves as configuration place for pointcoud segmentation dataset collection.
    /// Holds properties like directory, save frequency.
    /// </summary>
    [RequireComponent(typeof(RaycastLidar))]
    public class PointCloudSegmentationSaver : MonoBehaviour
    {

        /// <summary>
        /// Enable/Disable switch. Usefull for starting annotation during runtime.
        /// </summary>
        public bool Enable = false;

        /// <summary>
        /// Holds list of objects to track and annotate.
        /// </summary>
        public List<ClassObjects> ObjectClasses;

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

        [HideInInspector]
        public Dictionary<int, (int, int)> objectClassesAndInstances;

        private string _savePath;

        public void Start()
        {
            if (objectClassesAndInstances is not null)
                return;

            objectClassesAndInstances = new Dictionary<int, (int, int)>();
            CreateFolders();
            lidar = GetComponent<RaycastLidar>();
            _lastOrientation = lidar.transform.eulerAngles;
            _lastPosition = lidar.transform.position;
            counter = 0;
            int idx = 1;
            int classIdx = 1;
            // initialize collider id dictionary
            foreach (var cls in ObjectClasses)
            {
                foreach(var obj in cls.ObjectsInClass)
                {
                    var colliders = obj.GetComponentsInChildren<Collider>();
                    foreach(var col in colliders)
                    {
                        var colId = col.GetInstanceID();
                        objectClassesAndInstances.Add(colId, (classIdx, idx));
                    }
                    idx++;
                }
                classIdx++;
            }
        }

        public void Update()
        {
            if (Enable && SavePath != "" && (Time.time - _lastSave) >= (1 / SaveFrequencyHz))
            {
                Save(lidar.Points, lidar.Readings);
            }
        }

        /// <summary>
        /// Saves lidar points to PCD file,  generates point labels and saves them to file.
        /// Also writes lidar rotation matrix to poses.txt file.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="readings"></param>
        void Save(NativeArray<Vector3> points, NativeArray<LidarReading> readings)
        {
            var orientation = lidar.transform.eulerAngles - _lastOrientation;
            _lastOrientation = lidar.transform.eulerAngles;
            var orientationMatrix = Matrix4x4.Rotate(Quaternion.Euler(orientation));
            var translation = lidar.transform.position - _lastPosition;

            _lastPosition = lidar.transform.position;
            var c = counter.ToString("d6");
            var fileName = $"{c}";
            if (!string.IsNullOrWhiteSpace(Namespace))
                fileName = $"{Namespace}_{fileName}";

            var pcdFile = Path.Combine(_savePath, "lidar", fileName + ".pcd");
            Task t1 = Task.Run(() =>
            {
                var _filteredIndices = readings.Select((v, i) => new { v, i }).Where(x => x.v.IsValid).Select(x => x.i);
                var _filteredPoints = _filteredIndices.Select(m => points[m]).ToList();
                var _filteredReadings = _filteredIndices.Select(m => readings[m]).ToList();
                PCDSaver.WriteToPcdFileWithIntensity(pcdFile, _filteredPoints, _filteredReadings);
                WriteLabels(fileName, _filteredReadings);
                WritePose(orientationMatrix, translation);
            });
            counter++;
            _lastSave = Time.time;
        }

        void WritePose(Matrix4x4 matrix, Vector3 translation)
        {
            using (StreamWriter sw = File.AppendText(Path.Combine(_savePath, "poses.txt")))
            {
                sw.WriteLine($"{matrix[0,0]} {matrix[0,1]} {matrix[0,2]} {translation.x} {matrix[1,0]} {matrix[1,1]} {matrix[1,2]} {translation.y} {matrix[2,0]} {matrix[2,1]} {matrix[2,2]} {translation.z}");
            }
        }

        void WriteLabels(string fileName, List<LidarReading> readings)
        {
            var filePath = Path.Combine(_savePath, "labels", fileName + ".label");
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                foreach (var r in readings)
                {
                    var classId = BitConverter.GetBytes((ushort) r.ClassId);
                    var instId = BitConverter.GetBytes((ushort) r.InstanceId);
                    stream.Write(classId, 0, classId.Length);
                    stream.Write(instId, 0, instId.Length);
                }
            }
        }

        void CreateFolders()
        {
            if (string.IsNullOrWhiteSpace(SavePath))
            {
                SavePath = Path.Join(Application.dataPath, "pointcloud_segmentation");
            }
            Directory.CreateDirectory(SavePath);

            int idx = 0;
            string[] runs = Directory.GetDirectories(SavePath);
            if (runs.Length > 0)
            {
                foreach (var s in runs)
                {
                    var dir = new DirectoryInfo(s);
                    var dirName = dir.Name;
                    if (Int32.TryParse(dirName.Substring(3), out var _index))
                    {
                        if (_index > idx) idx = _index;
                    }
                }
            }
            idx++;
            _savePath = Path.Combine(SavePath, $"run{idx}");
            Directory.CreateDirectory(Path.Combine(_savePath, "labels"));
            Directory.CreateDirectory(Path.Combine(_savePath, "lidar"));
        }
    }
}
