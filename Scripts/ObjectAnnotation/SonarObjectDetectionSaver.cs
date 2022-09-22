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
using Marus.CustomInspector;


namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// This class serves as configuration place for pointcoud segmentation dataset collection.
    /// Holds properties like directory, save frequency.
    /// </summary>
    [RequireComponent(typeof(Sonar3D))]
    public class SonarObjectDetectionSaver : MonoBehaviour
    {

        /// <summary>
        /// Enable/Disable switch. Usefull for starting annotation during runtime.
        /// </summary>
        public bool Enable = false;

        /// <summary>
        /// If true, new images will be saved to last directory (last run)
        /// </summary>
        public bool ResumeFromLastTime = true;

        /// <summary>
        /// Holds list of objects to track and annotate.
        /// </summary>
        public List<ClassObjects> ObjectClasses;

        /// <summary>
        /// Saving frequency in Hz
        /// </summary>
        public float SaveFrequencyHz = 1f;


        /// <summary>
        /// Intensity threshold for pixel to be considered as part of an object.
        /// Ranges from 0 to 1.
        /// </summary>
        [Range(0, 1)]
        public float IntensityThreshold = 0.5f;

        [Header("Dataset setup", order=2)]
        [Space(10, order=3)]
        /// <summary>
        /// Directory for saving the dataset
        /// </summary>
        public string DatasetFolder;

        /// <summary>
        /// If enabled, creates test subset besides train and validation.
        /// </summary>
        public bool GenerateTestSubset = true;

        [Range(0, 1)]
        public float BackgroundImageSaveProbability = 0.05f;

        [Range(0, 100)]
        public int TrainSize = 75;
        [Range(0, 100)]
        public int ValSize = 10;
        [Range(0, 100)]
        [ConditionalHide("SplitTrainValTest", false)]
        public int TestSize = 15;

        [HideInInspector]
        public List<(int, string)> _classList;

        [HideInInspector]
        public List<ObjectRecord> ObjectsToTrack;

        private string _datasetPath;
        private string _imagesPath;
        private string _labelsPath;
        private List<Tuple<int, string>> _ratios;

        Sonar3D sonar;
        float _lastSave = 0;
        int counter;

        [HideInInspector]
        public Dictionary<int, (int, int)> objectClassesAndInstances;

        private string _savePath;

        public void Start()
        {
            objectClassesAndInstances = new Dictionary<int, (int, int)>();
            _ratios = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(TrainSize, "train"),
                new Tuple<int, string>(TestSize, "test"),
                new Tuple<int, string>(ValSize, "val")
            };
            _ratios.Sort(Comparer<Tuple<int, string>>.Default);
            _ratios.Reverse();

            counter = 0;
            int idx = 1;
            int classIdx = 1;

            _classList = new List<(int, string)>();

            // initialize collider id dictionary
            foreach (var cls in ObjectClasses)
            {
                _classList.Add((classIdx, cls.ClassName));
                idx = 1;
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
            CreateDatasetFolderStructure();
            sonar = GetComponent<Sonar3D>();
        }

        public void Update()
        {
            if (Enable && DatasetFolder != "" && (Time.time - _lastSave) >= (1 / SaveFrequencyHz))
            {
                Save(sonar.sonarCartesianImage, sonar.ClassInstanceImage);
                _lastSave = Time.time;
            }
        }

        /// <summary>
        /// Saves sonar image and corresponing labels in text file.
        /// Format is YOLOv5
        /// </summary>
        /// <param name="sonarImage"></param>
        /// <param name="classImage"></param>
        void Save(Texture2D sonarImage, Texture2D classImage)
        {
            int xMin, yMin;
            int xMax, yMax;
            bool hasObject = false;
            (string, string) subfolderIdx = GetSubfolderAndIndex();
            foreach((int, int) value in objectClassesAndInstances.Values.Distinct())
            {
                xMax = 0;
                yMax = 0;
                xMin = int.MaxValue;
                yMin  = int.MaxValue;
                bool found = false;
                for (int x = 0; x < classImage.height; x++)
                {
                    for (int y = 0; y < classImage.width; y++)
                    {
                        var pix = classImage.GetPixel(x, y);
                        if ((int) (pix.r * 255f) == value.Item1 && (int) (pix.g * 255f) == value.Item2 && pix.b >= IntensityThreshold && pix.b < 1f)
                        {
                            found = true;
                            hasObject = true;
                            xMin = Math.Min(x, xMin);
                            yMin = Math.Min(y, yMin);
                            xMax = Math.Max(x, xMax);
                            yMax = Math.Max(y, yMax);
                        }

                    }
                }

                if (found)
                {
                    var _lbl_path = Path.Combine(_labelsPath, subfolderIdx.Item1, subfolderIdx.Item2 + ".txt");

                    Vector2 center = new Vector2();
                    center.x = (xMin + xMax) / 2.0f;
                    center.y = (yMin + yMax) / 2.0f;

                    float _width = (float) Math.Abs(xMin - xMax);
                    float _height = (float) Math.Abs(yMin - yMax);

                    float x_center = center.x / (float) sonar.CartesianXRes;
                    float y_center = 1 - (center.y / (float) sonar.CartesianYRes);
                    float width = _width / (float) sonar.CartesianXRes;
                    float height = _height / (float) sonar.CartesianYRes;
                    using (StreamWriter sw = File.AppendText(_lbl_path))
                    {
                        sw.WriteLine($"{value.Item1} {x_center} {y_center} {width} {height}");
                    }
                }
            }
            float rand = UnityEngine.Random.Range(0f, 1f);
            if (hasObject || rand < BackgroundImageSaveProbability)
            {
                byte[] bytes = sonarImage.EncodeToPNG();
                var path = Path.Combine(_imagesPath, subfolderIdx.Item1, subfolderIdx.Item2 + ".png");
                File.WriteAllBytes(path, bytes);
            }


        }

        private (string,string) GetSubfolderAndIndex()
        {
            string prefix = "";
            string idx = "";
            float rand = UnityEngine.Random.Range(0f, 100f);

            // roulette wheel selection for train-test-val split based on predefined ratios
            if (rand < _ratios[0].Item1)
            {
                prefix = _ratios[0].Item2;
            }
            else if(rand >= _ratios[0].Item1 && rand < (_ratios[1].Item1 + _ratios[0].Item1))
            {
                prefix = _ratios[1].Item2;
            }
            else
            {
                prefix = _ratios[2].Item2;
            }

            if (prefix == "test" && !GenerateTestSubset)
            {
                prefix = "train";
            }

            string path = Path.Combine(_imagesPath, prefix);
            List<string> files = Directory.GetFiles(path, "*.png").Select(f => Path.GetFileName(f)).ToList();
            files.Sort();
            files.Reverse();
            if (files.Count == 0)
            {
                idx = "00001";
            }
            else
            {
                string num = files[0].Substring(0, files[0].IndexOf('.'));
                int currentIdx = Int32.Parse(num) + 1;
                idx = $"{currentIdx:00000}";
            }
            return (prefix, idx);
        }

        private void CreateDatasetFolderStructure()
        {
            if (string.IsNullOrWhiteSpace(DatasetFolder))
            {
                DatasetFolder = Path.Join(Application.dataPath, "sonar_detection");
            }
            Directory.CreateDirectory(DatasetFolder);
            int idx = 0;
            string[] runs = Directory.GetDirectories(DatasetFolder);
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
            if (!ResumeFromLastTime)
            {
                idx++;
            }
            _datasetPath = Path.Combine(DatasetFolder, $"run{idx}");

            _imagesPath = Path.Combine(_datasetPath, "images");
            Directory.CreateDirectory(_imagesPath);
            string trainPath = Path.Combine(_imagesPath, "train");
            Directory.CreateDirectory(trainPath);
            string valPath = Path.Combine(_imagesPath, "val");
            Directory.CreateDirectory(valPath);

            _labelsPath = Path.Combine(_datasetPath, "labels");
            Directory.CreateDirectory(_labelsPath);
            trainPath = Path.Combine(_labelsPath, "train");
            Directory.CreateDirectory(trainPath);
            valPath = Path.Combine(_labelsPath, "val");
            Directory.CreateDirectory(valPath);

            if (GenerateTestSubset)
            {
                string testPath = Path.Combine(_imagesPath, "test");
                Directory.CreateDirectory(testPath);

                testPath = Path.Combine(_labelsPath, "test");
                Directory.CreateDirectory(testPath);
            }

            string yamlPath = Path.Combine(_datasetPath, "simulation.yaml");

            using(var tw = new StreamWriter(yamlPath, false))
            {
                tw.WriteLine($"train: {Path.Combine(_imagesPath, "train")}");
                tw.WriteLine($"val: {Path.Combine(_imagesPath, "val")}");
                if (GenerateTestSubset)
                {
                    tw.WriteLine($"test: {Path.Combine(_imagesPath, "test")}");
                }
                tw.WriteLine($"nc: {_classList.Count}");
                _classList.Sort(Comparer<(int, string)>.Default);
                string _classListRepr = string.Join(",", _classList.Select(x => $"'{x.Item2}'"));
                tw.WriteLine($"names: [{_classListRepr}]");
            }
        }
    }
}
