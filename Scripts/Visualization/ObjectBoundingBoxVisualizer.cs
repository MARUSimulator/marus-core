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
using UnityEngine.UI;
using System.Collections.Generic;
using Marus.ObjectAnnotation;

namespace Marus.Visualization
{
	public class ObjectBoundingBoxVisualizer : MonoBehaviour
    {

        public List<Camera> Cameras;
        public List<GameObject> Objects;
        public GameObject VisualIndicator;
        public int VertexStep = 20;

        private Dictionary<int, GameObject> canvasMap;
        private List<GameObject> boundingBoxList;

        void Start()
        {
            boundingBoxList = new List<GameObject>();
            canvasMap = new Dictionary<int, GameObject>();
            foreach (var c in Cameras)
            {
                GameObject canvasGO = new GameObject();
                canvasGO.name = "VisualizationCanvas";
                canvasGO.AddComponent<Canvas>();

                Canvas myCanvas = canvasGO.GetComponent<Canvas>();
                myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                myCanvas.targetDisplay = c.targetDisplay;
                canvasGO.hideFlags = HideFlags.HideInHierarchy;
                canvasMap.Add(myCanvas.targetDisplay, canvasGO);
            }
        }

        void Update()
        {
            foreach (GameObject go in boundingBoxList)
            {
                Destroy(go);
            }
            boundingBoxList.Clear();
            foreach (GameObject go in Objects)
            {
                foreach (Camera c in Cameras)
                {
                    Rect boundingBox = new Rect();
                    try
                    {
                        boundingBox = CameraObjectDetectionSaver.GetBoundingBoxFromMesh(go, c);
                    }
                    catch
                    {
                        continue;
                    }
                    GameObject parent = canvasMap[c.targetDisplay];
                    boundingBoxList.Add(VisualizeObjectBounds(go, boundingBox, c, parent));
                }
            }
        }


        private GameObject VisualizeObjectBounds(GameObject obj, Rect bounds, Camera CameraView, GameObject parent)
        {
            GameObject rect = Instantiate(VisualIndicator, Vector3.zero, Quaternion.identity, parent.transform);
            RectTransform rt = rect.transform.Find("SelectionImage").GetComponent<RectTransform>();
            rt.position = new Vector2(bounds.center.x, bounds.center.y);
            rt.sizeDelta = new Vector2(bounds.width * 1.1f, bounds.height * 1.1f);
            return rect;
        }
    }
}
