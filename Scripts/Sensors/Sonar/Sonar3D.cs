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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Networking;
using Labust.Sensors;
using Labust.Sensors.Core;
using Labust.Visualization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using Sensorstreaming;
using System.Threading;
using Labust.Core;


namespace Labust.Sensors
{

    /// <summary>
    /// Sonar that cast N rays evenly distributed in configured field of view.
    /// Implemented using IJobParallelFor on CPU
    /// Can drop performance
    /// </summary>
    public class Sonar3D : SensorBase
    {

        /// Instantiates 3 Jobs:
        /// 1) RaycastCommand creation - create raycast commands <see cref="RaycastCommand"> for lidar FoV
        /// 2) RaycastCommand execution
        /// 3) RaycastHit data interpretation - extract points, distances etc.  



        /// <summary>
        /// Material set for point cloud display
        /// </summary>
        public Material ParticleMaterial;

        public int WidthRes = 30;

        public int HeightRes = 10;
        public float MaxDistance = float.MaxValue;
        public float MinDistance = 0;
        public float HorizontalFieldOfView = 120;
        public float VerticalFieldOfView = 15;

        public int imageHeight = 256;

        public bool IsIdeal = false;

        [ConditionalHideInInspector("IsIdeal", true)]
        public float RayIntensity = 40;

        int NumRaysPerAccusticRay = 1; // 1, 5, 9 TODO
                                       // float rayWidth = 0.001f; // in radians

        public ComputeShader pointCloudShader;
        public NativeArray<Vector3> pointsCopy;
        public NativeArray<SonarReading> sonarData;

        const float PIOVERTWO = Mathf.PI / 2;
        const float TWOPI = Mathf.PI * 2;
        const float WATER_LEVEL = 0;
        //public Image sonarImage;
        public RawImage sonarDisplay;
        // Start is called before the first frame update
        PointCloudManager _pointCloudManager;
        RaycastJobHelper<SonarReading> _raycastHelper;
        Coroutine _coroutine;
        Vector3 sonarPosition;
        Texture2D sonarImage;

        void Start()
        {

            int totalRays = WidthRes * HeightRes * NumRaysPerAccusticRay;
            Texture2D sonarImage = new Texture2D(WidthRes,imageHeight);

            pointsCopy = new NativeArray<Vector3>(totalRays, Allocator.Persistent);
            sonarData = new NativeArray<SonarReading>(totalRays, Allocator.Persistent);

            var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(WidthRes, HeightRes, HorizontalFieldOfView, VerticalFieldOfView);
            _raycastHelper = new RaycastJobHelper<SonarReading>(gameObject, directionsLocal, OnSonarHit, OnFinish, MaxDistance);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointClout", totalRays, ParticleMaterial, pointCloudShader);

            _coroutine = StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        protected override void SampleSensor()
        {
            _pointCloudManager.UpdatePointCloud(pointsCopy);
            sonarPosition = transform.position;
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<SonarReading> reading)
        {
            points.CopyTo(pointsCopy);
            hasData = true;

            //Debug.Log(reading[1].Intensity);
            Color pixel;
            reading.CopyTo(sonarData); 
            Texture2D sonarImage = new Texture2D(WidthRes,HeightRes); //new Texture2D(WidthRes,imageHeight);

            float[] yIntensity = new float[imageHeight];
            int yCoordinate;


            //composing a "photographic" x-y image
            // for (var y = 0; y < HeightRes; y++)
            // {
            //     for (var x = 0; x < WidthRes; x++)
            //     {
            //         pixel = new Color(reading[y * WidthRes + x].Intensity, reading[y * WidthRes + x].Intensity, reading[y * WidthRes + x].Intensity, 1);
            //         //normalizing the intensity to 0-255
            //         //pixel = pixel / reading.Intensity.* 256;
            //         sonarImage.SetPixel(y, x, pixel);
            //     }
            // }

            //composing a sonar image
            for (var x = 0; x < WidthRes; x++)
            {
                for (var y = 0; y < HeightRes; y++)
                {
                    yCoordinate = DistanceToImageY(reading[x * WidthRes + y].Distance);
                    yIntensity[yCoordinate] += reading[x * WidthRes + y].Intensity;
                }
                for (var y = 0; y < imageHeight; y++)
                {
                    pixel = new Color(yIntensity[y], yIntensity[y], yIntensity[y], 1);
                    sonarImage.SetPixel(y, x, pixel);
                }
                Array.Clear(yIntensity, 0, yIntensity.Length);
                                    
            }

            sonarImage.Apply();
            sonarImage.EncodeToJPG();
            sonarDisplay.texture = sonarImage;

            //Debug.Log("Sonar frame recorded!");
        }

        private int DistanceToImageY(float distance)
        {
            int y = (int)Math.Round((distance - MinDistance) / (MaxDistance - MinDistance) * imageHeight);
            return y;
        }

        void OnDestroy()
        {
            _raycastHelper.Dispose();
            pointsCopy.Dispose();
            sonarData.Dispose();
        }


        public SonarReading OnSonarHit(RaycastHit hit, Vector3 direction, int i)
        {
            var distance = hit.distance;
            var sonarReading = new SonarReading();
            if (distance < MinDistance || hit.point.y > WATER_LEVEL) // if above water, it is not hit!
            {
                sonarReading.Valid = false;
            }
            else
            {
                sonarReading.Valid = true;
                sonarReading.Distance = hit.distance;

                sonarReading.Intensity = RayIntensity * (Math.Abs(Vector3.Dot(direction, hit.normal)) / hit.distance);
                //sonarReading.Intensity = RayIntensity * (Vector3.Dot(direction, hit.normal)) / Mathf.Pow(hit.distance, 4);
                //Debug.Log("Target hit");
                //Debug.DrawRay(sonarPosition, direction*hit.distance, Color.white, 5.0f);

            }
            return sonarReading;
        }

    }

}