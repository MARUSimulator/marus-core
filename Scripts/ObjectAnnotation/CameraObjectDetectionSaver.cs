// Copyright 2021 Laboratory for Underwater Systems and Technologies (LABUST)
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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;
using Marus.CustomInspector;

namespace Marus.ObjectAnnotation
{
    /// <summary>
    /// Holds list of objects beloging to particular class.
    /// </summary>
    [Serializable]
    public class ClassObjects
    {
        public string ClassName;
        public List<GameObject> ObjectsInClass;
    }

    public class ObjectRecord
    {
        public int ClassIndex;
        public GameObject Object;
    }

    /// <summary>
    /// Main component for object annotation in camera images.
    /// Manages objects and cameras, dataset properties etc.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class CameraObjectDetectionSaver : MonoBehaviour
    {
        /// <summary>
        /// Enable/Disable switch. Usefull for starting annotation during runtime.
        /// </summary>
        public bool Enable = true;

        /// <summary>
        /// If true, new images will be saved to last directory (last run)
        /// </summary>
        public bool ResumeFromLastTime = true;

        [Header("Objects and Cameras Setup", order=0)]
        /// <summary>
        /// Holds list of objects to track and annotate.
        /// </summary>
        public List<ClassObjects> ObjectClasses;

        /// <summary>
        /// Holds list of cameras to annotate objects from.
        /// </summary>
        public List<Camera> CameraViews;

        /// <summary>
        /// Annotation frequency in Hz.
        /// </summary>
        public float SaveFrequencyHz = 1f;

        /// <summary>
        /// Area threshold for object to be annotated.
        /// In pixes squared.
        /// </summary>
        [Tooltip("Minimum area of object bounding box to be logged (in pixels)")]
        public float MinimumObjectArea = 256f;

        /// <summary>
        /// MinVerticalPosition (eg. water surface y value) is used for ignoring objects, or parts of object that are under water (not visible).
        /// </summary>
        [Tooltip("MinVerticalPosition (eg. water surface y value) is used for ignoring objects, or parts of object that are below this value.")]
        public float MinVerticalPosition = 0f;

        /// <summary>
        /// Use raycasting for more precise results.
        /// </summary>
        [Tooltip("Use Raycasting for more precise results. If enabled, objects must have mesh collider.")]
        [HideInInspector]
        public bool RaycastCheck = true;

        /// <summary>
        /// Step used when iterating over object vertices.
        /// Bigger step means less precision but better performance.
        /// </summary>
        [Tooltip("For optimization. Higher number is faster but less precise.")]
        public int VertexStep = 20;

        [Space(20)]

        [Header("Dataset setup", order=2)]
        [Space(10, order=3)]
        /// <summary>
        /// Directory for saving the dataset
        /// </summary>
        public string DatasetFolder;

        /// <summary>
        /// Image width
        /// It should correspond to display resolution in Game Mode.
        /// </summary>
        [ReadOnly]
        public int ImageWidth = 1920;
        /// <summary>
        /// Image height
        /// It should correspond to display resolution in Game Mode.
        /// </summary>
        [ReadOnly]
        public int ImageHeight = 1080;

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
        private GameObject _parent;
        private List<GameObject> _objList;

        [HideInInspector]
        public List<(int, string)> _classList;

        [HideInInspector]
        public List<ObjectRecord> ObjectsToTrack;

        private string _imagesPath;
        private string _labelsPath;

        private List<Tuple<int, string>> _ratios;
        private float _timer;
        private List<Tuple<ObjectRecord, Rect>> _objectsInScene;
        private string _datasetPath;

        void Start()
        {

            ObjectsToTrack = new List<ObjectRecord>();
            _objectsInScene = new List<Tuple<ObjectRecord, Rect>>();
            _timer = 0f;
            _parent = GameObject.Find("SelectionCanvas");
            _objList = new List<GameObject>();
            _classList = new List<(int, string)>();
            List<string> _tmpList = new List<string>();
            int idx = 0;
            foreach(var r in ObjectClasses)
            {
                if (_tmpList.Contains(r.ClassName)) continue;
                _tmpList.Add(r.ClassName);
                _classList.Add((idx, r.ClassName));
                foreach (var obj in r.ObjectsInClass)
                {
                    ObjectsToTrack.Add(new ObjectRecord()
                    {
                        ClassIndex = idx,
                        Object = obj
                    });
                }
                idx++;
            }

            CreateDatasetFolderStructure();
            _ratios = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(TrainSize, "train"),
                new Tuple<int, string>(TestSize, "test"),
                new Tuple<int, string>(ValSize, "val")
            };
            _ratios.Sort(Comparer<Tuple<int, string>>.Default);
            _ratios.Reverse();
        }

        void Update()
        {
            if(!Enable)
            {
                return;
            }

#if UNITY_EDITOR
            string[] res = UnityStats.screenRes.Split('x');
            ImageWidth =  int.Parse(res[0]);
            ImageHeight = int.Parse(res[1]);
#else
            ImageWidth =  Screen.width;
            ImageHeight = Screen.height;
#endif
            foreach(GameObject o in _objList)
            {
                Destroy(o);
            }
            _objList.Clear();

            if (_timer >= (1 / SaveFrequencyHz))
            {
                StartCoroutine("CaptureAndSaveImage");
                _timer = 0f;
            }
            _timer += Time.deltaTime;
        }

        IEnumerator CaptureAndSaveImage()
        {
            _objectsInScene.Clear();
            yield return new WaitForEndOfFrame();
            foreach(Camera CameraView in CameraViews)
            {
                foreach (ObjectRecord o in ObjectsToTrack)
                {
                    GameObject obj = o.Object;
                    Rect boundingBox;
                    try
                    {
                        boundingBox = GetBoundingBoxFromMesh(obj, CameraView, VertexStep, RaycastCheck:RaycastCheck, VerticalPositionLimit:MinVerticalPosition);
                    }
                    catch
                    {
                        continue;
                    }

                    if (boundingBox != null && (boundingBox.size.x * boundingBox.size.y) > MinimumObjectArea)
                    {
                        _objectsInScene.Add(new Tuple<ObjectRecord, Rect>(o, boundingBox));
                    }
                }

                float rand = UnityEngine.Random.Range(0f, 1f);


                if (_objectsInScene.Count > 0 || rand < BackgroundImageSaveProbability)
                {
                    CameraView.targetTexture = RenderTexture.GetTemporary(ImageWidth, ImageHeight, 16);
                    RenderTexture r = CameraView.targetTexture;
                    RenderTexture currentRT = RenderTexture.active;
                    RenderTexture.active = r;
                    CameraView.Render();
                    AsyncGPUReadback.Request(r, 0, TextureFormat.RGBA32, OnCompleteReadback);
                    RenderTexture.ReleaseTemporary(r);
                    CameraView.targetTexture = null;
                }
            }
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest asyncGPUReadbackRequest)
        {
            if (asyncGPUReadbackRequest.hasError)
            {
                Debug.LogError("Error Capturing Screenshot: With AsyncGPUReadbackRequest.");
                return;
            }
            var newTex = new Texture2D
            (
                ImageWidth,
                ImageHeight,
                TextureFormat.RGBA32,
                false
            );
            var rawData = asyncGPUReadbackRequest.GetData<byte>();
            newTex.LoadRawTextureData(rawData);
            newTex.Apply();
            (string, string) subfolderIdx = GetSubfolderAndIndex();
            string savePath = Path.Combine(_imagesPath, subfolderIdx.Item1, subfolderIdx.Item2 + ".jpg");
            File.WriteAllBytes(savePath, ImageConversion.EncodeToJPG(newTex));
            Destroy(newTex);
            rawData.Dispose();
            SaveLabel(_objectsInScene, subfolderIdx.Item1, subfolderIdx.Item2);
        }

        private void SaveLabel(List<Tuple<ObjectRecord, Rect>> objects, string folder, string index)
        {
            var path = Path.Combine(_labelsPath, folder, index + ".txt");
            if (objects.Count == 0)
            {
                File.Create(path);
            }
            foreach(var o in objects)
            {
                float x_center = o.Item2.center.x / (float) ImageWidth;
                float y_center = 1 - (o.Item2.center.y / (float) ImageHeight);
                float width = o.Item2.width / (float) ImageWidth;
                float height = o.Item2.height / (float) ImageHeight;
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine($"{o.Item1.ClassIndex} {x_center} {y_center} {width} {height}");
                }
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
            List<string> files = Directory.GetFiles(path, "*.jpg").Select(f => Path.GetFileName(f)).ToList();
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


        /// <summary>
        /// Calculates bounding box of object in camera view based on mesh vertices.
        /// </summary>
        /// <param name="gameObject">Game object</param>
        /// <param name="CameraView">Camera object</param>
        /// <param name="VertexStep">Vertex Step</param>
        /// <param name="VerticalPositionLimit">Vertical position limit. Vertices below will be ignored</param>
        /// <param name="RaycastCheck">Use raycast checking for more precise bounding boxes.</param>
        /// <returns>Bounding box of object in camera.</returns>
        public static Rect GetBoundingBoxFromMesh(GameObject gameObject, Camera CameraView, int VertexStep = 20, float VerticalPositionLimit = 0, bool RaycastCheck = true)
        {
            List<Vector3> vertices;
            Rect retVal = Rect.MinMaxRect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            bool visible = false;
            var meshes = gameObject.GetComponentsInChildren<MeshFilter>();


            foreach(MeshFilter mesh in meshes)
            {

                var r = mesh.gameObject.GetComponent<Renderer>();
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraView);
                if(!GeometryUtility.TestPlanesAABB(planes, r.bounds))
                {
                    continue;
                }

                vertices = new List<Vector3>();
                vertices.AddRange(mesh.sharedMesh.vertices);
                for (int i = 0; i < vertices.Count; i = i + VertexStep) {
                    Vector3 vertWorld = mesh.gameObject.transform.TransformPoint(vertices[i]);
                    // hide underwater parts
                    if (vertWorld.y < VerticalPositionLimit)
                    {
                        continue;
                    }
                    Vector3 screenView = CameraView.WorldToScreenPoint(vertWorld);
                    if (screenView.x < Screen.width && screenView.x > 0 &&
                        screenView.y < Screen.height && screenView.y > 0
                        && screenView.z > 0)
                    {
                        if (!RaycastCheck)
                        {
                            visible = true;
                        }
                        else
                        {
                            RaycastHit hit;
                            if (UnityEngine.Physics.Linecast(CameraView.transform.position, vertWorld, out hit))
                            {

                                if (hit.collider.gameObject.GetInstanceID() == mesh.gameObject.GetInstanceID())
                                {
                                    visible = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    if (screenView.x < retVal.xMin) {
                        retVal.xMin = screenView.x ;
                    }

                    if (screenView.y < retVal.yMin) {
                        retVal.yMin = screenView.y;
                    }

                    if (screenView.x > retVal.xMax) {
                        retVal.xMax = screenView.x;
                    }

                    if (screenView.y > retVal.yMax) {
                        retVal.yMax = screenView.y;
                    }
                }
            }

            if (!visible)
            {
                throw new System.Exception("Object is not visible on camera");
            }

            return retVal;
        }

        private void CreateDatasetFolderStructure()
        {
            if (string.IsNullOrWhiteSpace(DatasetFolder))
            {
                DatasetFolder = Path.Join(Application.dataPath, "camera_detection");
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
