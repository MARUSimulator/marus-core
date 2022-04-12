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
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;

namespace Marus.ObjectAnnotation
{
    [Serializable]
    public class ObjectRecord
    {
        public GameObject Object;
        public string Class;
    }

    public class ObjectAnnotator : MonoBehaviour
    {
        public bool Enable = false;
        [Header("Objects & Cameras Setup", order=0)]
        [Space(10, order=1)]
        public List<ObjectRecord> ObjectsToTrack;

        public List<Camera> CameraViews;
        public float SampleRateHz = 1f;
        [Tooltip("Minimum area of object bounding box to be logged (in pixels)")]
        public float MinimumObjectArea = 256f;

        [Tooltip("Use Raycasting for more precise results. If enabled, objects must have mesh collider.")]
        public bool RaycastCheck = true;
        [Tooltip("For optimization. Higher number is faster but less precise.")]
        public int VertexStep = 20;

        [Space(20)]

        [Header("Dataset setup", order=2)]
        [Space(10, order=3)]
        public string DatasetFolder;
        public int ImageWidth = 1920;
        public int ImageHeight = 1080;
        public bool SplitTrainValTest;

        [Range(50,99)]
        [ConditionalHide("SplitTrainValTest", false)]
        public int TrainSize = 75;
        [Range(1,49)]
        [ConditionalHide("SplitTrainValTest", false)]
        public int TestSize = 15;
        private GameObject _parent;
        private List<GameObject> _objList;
        private List<string> _classList;

        private string _imagesPath;
        private string _labelsPath;

        private List<Tuple<int, string>> _ratios;
        private int _valSize;
        private float _timer;
        private List<Tuple<ObjectRecord, Rect>> _objectsInScene;

        void Start()
        {
            _objectsInScene = new List<Tuple<ObjectRecord, Rect>>();
            _timer = 0f;
            _parent = GameObject.Find("SelectionCanvas");
            _objList = new List<GameObject>();
            _classList = new List<string>();
            foreach (ObjectRecord o in ObjectsToTrack)
            {
                if (!_classList.Contains(o.Class))
                {
                    _classList.Add(o.Class);
                }
            }

            if (DatasetFolder != "")
            {
                CreateDatasetFolderStructure();
            }

            _valSize = 100 - TrainSize - TestSize;
            _ratios = new List<Tuple<int, string>>()
            {
                new Tuple<int, string>(TrainSize, "train"),
                new Tuple<int, string>(TestSize, "test"),
                new Tuple<int, string>(_valSize, "val")
            };
            _ratios.Sort(Comparer<Tuple<int, string>>.Default);
            _ratios.Reverse();
        }

        void Update()
        {
            foreach(GameObject o in _objList)
            {
                Destroy(o);
            }
            _objList.Clear();

            if (Enable && _timer >= (1 / SampleRateHz))
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
                        boundingBox = GetBoundingBoxFromMesh(obj, CameraView, VertexStep, RaycastCheck:RaycastCheck);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.StackTrace);
                        continue;
                    }

                    if (boundingBox != null && (boundingBox.size.x * boundingBox.size.y) > MinimumObjectArea)
                    {
                        _objectsInScene.Add(new Tuple<ObjectRecord, Rect>(o, boundingBox));
                    }
                }
                if (_objectsInScene.Count > 0)
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
            foreach(var o in objects)
            {
                int classIdx = _classList.IndexOf(o.Item1.Class);
                float x_center = o.Item2.center.x / (float) ImageWidth;
                float y_center = 1 - (o.Item2.center.y / (float) ImageHeight);
                float width = o.Item2.width / (float) ImageWidth;
                float height = o.Item2.height / (float) ImageHeight;
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine($"{classIdx} {x_center} {y_center} {width} {height}");
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

            if (prefix == "test" && !SplitTrainValTest)
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

        public static Rect GetBoundingBoxFromMesh(GameObject go, Camera CameraView, int VertexStep = 50, int VerticalPositionLimit = 0, bool RaycastCheck = false)
        {
            List<Vector3> vertices;
            Rect retVal = Rect.MinMaxRect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            bool visible = false;
            var meshes = go.GetComponentsInChildren<MeshFilter>().ToList();
            var originmesh = go.GetComponent<MeshFilter>();
            if (originmesh != null)
            {
                meshes.Add(originmesh);
            }
            foreach(MeshFilter mesh in meshes)
            {
                vertices = new List<Vector3>();
                vertices.AddRange(mesh.mesh.vertices);
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
            Directory.CreateDirectory(DatasetFolder);

            _imagesPath = Path.Combine(DatasetFolder, "images");
            Directory.CreateDirectory(_imagesPath);
            string trainPath = Path.Combine(_imagesPath, "train");
            Directory.CreateDirectory(trainPath);
            string valPath = Path.Combine(_imagesPath, "val");
            Directory.CreateDirectory(valPath);

            _labelsPath = Path.Combine(DatasetFolder, "labels");
            Directory.CreateDirectory(_labelsPath);
            trainPath = Path.Combine(_labelsPath, "train");
            Directory.CreateDirectory(trainPath);
            valPath = Path.Combine(_labelsPath, "val");
            Directory.CreateDirectory(valPath);

            if (SplitTrainValTest)
            {
                string testPath = Path.Combine(_imagesPath, "test");
                Directory.CreateDirectory(testPath);

                testPath = Path.Combine(_labelsPath, "test");
                Directory.CreateDirectory(testPath);
            }

            string yamlPath = Path.Combine(DatasetFolder, "simulation.yaml");

            using(var tw = new StreamWriter(yamlPath, false))
            {
                tw.WriteLine($"train: {Path.Combine(_imagesPath, "train")}");
                tw.WriteLine($"val: {Path.Combine(_imagesPath, "val")}");
                if (SplitTrainValTest)
                {
                    tw.WriteLine($"test: {Path.Combine(_imagesPath, "test")}");
                }
                tw.WriteLine($"nc: {_classList.Count}");
                string _classListRepr = string.Join(",", _classList.Select(x => $"'{x}'"));
                tw.WriteLine($"classes: [{_classListRepr}]");
            }
        }
    }
}
