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
using Marus.NoiseDistributions;
using UnityEditor;

namespace Marus.Visualization
{

    [DefaultExecutionOrder(100)]
	public class ObjectBoundingBoxVisualizer : MonoBehaviour
    {

        public CameraObjectDetectionSaver Annotator;
        public GameObject VisualIndicator;
        public NoiseParameters boundingBoxNoise;
        public int VertexStep = 20;

        private Dictionary<int, GameObject> canvasMap;
        private List<GameObject> boundingBoxList;

        private List<Camera> Cameras;
        private List<ObjectRecord> Objects;
        private List<(int, string)> _classes;

        void Setup()
        {
            if (Annotator is null) return;

            Cameras = Annotator.CameraViews;
            Objects = Annotator.ObjectsToTrack;
            _classes = Annotator._classList;
            Debug.Log(Annotator);
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

        void Start()
        {
            Setup();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (Annotator is null || boundingBoxList is null)
            {
                return;
            }

            if (Objects is null)
            {
                Setup();
            }

            foreach (GameObject go in boundingBoxList)
            {
                Destroy(go);
            }
            boundingBoxList.Clear();
            foreach (ObjectRecord go in Objects)
            {
                foreach (Camera c in Cameras)
                {
                    Rect boundingBox = new Rect();
                    try
                    {
                        boundingBox = CameraObjectDetectionSaver.GetBoundingBoxFromMesh(go.Object, c);
                    }
                    catch
                    {
                        continue;
                    }
                    VisualizeObjectBounds(go.Object, boundingBox, c, _classes[go.ClassIndex].Item2);
                }
            }
        }


        private void VisualizeObjectBounds(GameObject obj, Rect bounds, Camera CameraView, string className="")
        {
            if ((bounds.width * bounds.height) < 8000) return;
            var ld = new Vector3(bounds.center.x - bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y - bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            var dd = new Vector3(bounds.center.x + bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y - bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            var lg = new Vector3(bounds.center.x - bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y + bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            var dg = new Vector3(bounds.center.x + bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y + bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            Gizmos.color = Color.red;
            var pixelRatio = UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x - UnityEditor.HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;
            UnityEditor.Handles.BeginGUI();
            UnityEditor.Handles.color = Color.red;
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)15,
                normal = new GUIStyleState() { textColor = Color.white }
            };
            Vector2 size = style.CalcSize(new GUIContent(name));
            var pos = UnityEditor.HandleUtility.WorldToGUIPoint(obj.transform.position) + new Vector2(100, -50);
            var scr = bounds.center;
            Vector2 convertedGUIPos = GUIUtility.ScreenToGUIPoint(scr);

            if (className == "")
            {
                className = obj.name;
            }
            // var r = Random.Range(0f, 1f);

            // string[] names = new string[] {"Vessel A", "Vessel B", "Vessel C", "Vessel D","Vessel E","Vessel F", "Vessel G"};

            // if (r > 0.9)
            // {
            //     var r2 = (int) Random.Range(0, 6);
            //     className = names[r2];
            // }
            GUI.Label(new Rect(lg.x, Screen.height - lg.y - 50, bounds.width, 50), className, style);

            var canvas = canvasMap[CameraView.targetDisplay].GetComponent<Canvas>();
            ScreenGizmos.DrawLine(canvas, CameraView, ld, dd);
            ScreenGizmos.DrawLine(canvas, CameraView, ld, lg);
            ScreenGizmos.DrawLine(canvas, CameraView, dd, dg);
            ScreenGizmos.DrawLine(canvas, CameraView, lg, dg);

            UnityEditor.Handles.EndGUI();
        }
    }
}

public static class ScreenGizmos
{
	private const float offset = 0.001f;

	/// <summary>
	/// Draws a line in screen space between the 
	/// <paramref name="startPixelPos"/> and the 
	/// <paramref name="endPixelPos"/>. 
	/// </summary>
	public static void DrawLine(
		Canvas canvas, 
		Camera camera, 
		Vector3 startPixelPos, 
		Vector3 endPixelPos)
	{
		if (camera == null || canvas == null)
			return;

		Vector3 startWorld = PixelToCameraClipPlane(
			camera, 
			canvas, 
			startPixelPos);

		Vector3 endWorld = PixelToCameraClipPlane(
			camera, 
			canvas, 
			endPixelPos);

		Gizmos.DrawLine(startWorld, endWorld);
	}

	/// <summary>
	/// Converts the <paramref name="screenPos"/> to world space 
	/// near the <paramref name="camera"/> near clip plane. The 
	/// z component of the <paramref name="screenPos"/> 
	/// will be overriden.
	/// </summary>
	private static Vector3 PixelToCameraClipPlane(
		Camera camera, 
		Canvas canvas,
		Vector3 screenPos)
	{
		// The canvas scale factor affects the
		// screen position of all UI elements.
		screenPos *= canvas.scaleFactor;

		// The z-position defines the distance to the camera
		// when using Camera.ScreenToWorldPoint.
		screenPos.z = camera.nearClipPlane + offset;
		return camera.ScreenToWorldPoint(screenPos);
	}
}