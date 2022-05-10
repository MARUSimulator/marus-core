using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

        /// <summary>
        /// Material set for point cloud display
        /// </summary>
        public Material ParticleMaterial;
        public int WidthRes = 256;
        public int HeightRes = 256;
        public float MaxDistance = 30;
        public float MinDistance = 0.6F;
        public float HorizontalFieldOfView = 60;
        public float VerticalFieldOfView = 30;

        public int imageHeight = 256;
        public int CartesianXRes = 256;
        public int CartesianYRes = 256;
        double thetha;
        double r;
        enum RayDistribution { equiangular, equidistant }

        [SerializeField] RayDistribution rayDistribution;

        public bool IsIdeal = false;
        [ConditionalHideInInspector("IsIdeal", true)]
        public float RayIntensity = 0.1F;
        int NumRaysPerAccusticRay = 1; // 1, 5, 9 TODO
                                       // float rayWidth = 0.001f; // in radians

        public ComputeShader pointCloudShader;
        public NativeArray<Vector3> pointsCopy;
        public NativeArray<SonarReading> sonarData;
        const float WATER_LEVEL = 0;
        //public Image sonarImage;
        float altitude, pitch;
        public RawImage sonarDisplay;
        public RawImage sonarPhotoDisplay;
        public RawImage sonarCartesianDisplay;
        // Start is called before the first frame update
        PointCloudManager _pointCloudManager;
        RaycastJobHelper<SonarReading> _raycastHelper;
        Coroutine _coroutine;
        Vector3 sonarPosition;
        public Texture2D sonarImage, sonarPhotoImage, sonarCartesianImage;

        void Start()
        {

            int totalRays = WidthRes * HeightRes * NumRaysPerAccusticRay;

            sonarImage = new Texture2D(WidthRes, imageHeight);
            sonarPhotoImage = new Texture2D(WidthRes, HeightRes);
            sonarCartesianImage = new Texture2D(CartesianXRes, CartesianYRes);

            pointsCopy = new NativeArray<Vector3>(totalRays, Allocator.Persistent);
            sonarData = new NativeArray<SonarReading>(totalRays, Allocator.Persistent);

            //sample depth under the sonar for equidistant ray projection
            RaycastHit hit;
            if (UnityEngine.Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
            {
                altitude = hit.distance;
            }
            //get the pitch angle from the sonar frame
            pitch = transform.eulerAngles.x;

            //if (RayDistribution.equiangular)
            //{
            //    var directionsLocal = RaycastJobHelper.EvenlyDistributeRays(WidthRes, HeightRes, HorizontalFieldOfView, VerticalFieldOfView);
            //}
           // else if (RayDistribution.equidistant)
            //{
                var directionsLocal = RaycastJobHelper.EquidistantRays(WidthRes, HeightRes, HorizontalFieldOfView, VerticalFieldOfView, altitude, pitch);
            //}

            _raycastHelper = new RaycastJobHelper<SonarReading>(gameObject, directionsLocal, OnSonarHit, OnFinish, MaxDistance);

            _pointCloudManager = PointCloudManager.CreatePointCloud(name + "_PointClout", totalRays, ParticleMaterial, pointCloudShader);

            _coroutine = StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        protected override void SampleSensor()
        {
            _pointCloudManager.UpdatePointCloud(pointsCopy);
            sonarPosition = transform.position;
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<SonarReading> sonarReadings)
        {
            points.CopyTo(pointsCopy);
            sonarReadings.CopyTo(sonarData);

            ComposePhotoImage(sonarReadings);
            ComposePolarImage(sonarReadings);
            ComposeCartesianImage(sonarReadings);

            hasData = true;
        }

        private int DistanceToImageY(float distance)
        {
            if (distance < MaxDistance && distance >= MinDistance)
            {
                int y = (int)Math.Floor(((distance - MinDistance) / (MaxDistance - MinDistance)) * imageHeight);
                return y;
            }
            else
            {
                return 0;
            }
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
            if (distance < MinDistance || hit.point.y > WATER_LEVEL || hit.point == Vector3.zero) // if above water, it is not hit!
            {
                sonarReading.Valid = false;
            }
            else
            {
                sonarReading.Valid = true;
                sonarReading.Distance = hit.distance;

                sonarReading.Intensity = RayIntensity * (float)(Math.Acos(Math.Abs(Vector3.Dot(direction, hit.normal))));
                //sonarReading.Intensity = RayIntensity * (Vector3.Dot(direction, hit.normal)) / Mathf.Pow(hit.distance, 4);

            }
            return sonarReading;
        }

                private void ComposePhotoImage(NativeArray<SonarReading> reading)
        {
            Color pixel;
            for (var x = 0; x < WidthRes; x++)
            {
                for (var y = 0; y < HeightRes; y++)
                {
                    if (reading[x * HeightRes + y].Valid)
                    {
                        pixel = new Color(reading[x * HeightRes + y].Intensity, reading[x * HeightRes + y].Intensity, reading[x * HeightRes + y].Intensity, 1);
                    }
                    else
                    {
                        pixel = new Color(0, 0, 0, 1);
                    }
                    //normalizing the intensity to 0-255
                    //pixel = pixel / reading.Intensity.* 256;
                    sonarPhotoImage.SetPixel(x, y, pixel);
                }
            }

            sonarPhotoImage.Apply();
            sonarPhotoImage.EncodeToJPG();
            sonarPhotoDisplay.texture = sonarPhotoImage;
        }
        private void ComposePolarImage(NativeArray<SonarReading> reading)
        {
            Color pixel;
            int xCoordinate, yCoordinate;
            float[] yIntensity = new float[imageHeight];
            for (var x = 0; x < WidthRes; x++)
            {
                //squashing all spatial columns into 2D and adding the intensities
                for (var y = 0; y < HeightRes; y++)
                {
                    yCoordinate = DistanceToImageY(reading[x * HeightRes + y].Distance);
                    if (yCoordinate >= imageHeight)
                        Debug.Log("y coordinate = " + yCoordinate);
                    yIntensity[yCoordinate] += reading[x * HeightRes + y].Intensity;
                }

                //stacking the intensities into corresponding 2D image columns
                for (var y = 0; y < imageHeight; y++)
                {
                    pixel = new Color(yIntensity[y], yIntensity[y], yIntensity[y], 1);
                    sonarImage.SetPixel(x, y, pixel);
                }
                Array.Clear(yIntensity, 0, yIntensity.Length);
            }

            sonarImage.Apply();
            byte[] bytes = sonarImage.EncodeToPNG();
            var dirPath = Application.dataPath + "/../SaveImages/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "ImagePolar" + ".png", bytes);
            sonarDisplay.texture = sonarImage;
        }
        private void ComposeCartesianImage(NativeArray<SonarReading> reading)
        {
            Color pixel;
            int xCoordinate, yCoordinate;
            
            //populate left side of the swath
            for (var x = CartesianXRes / 2; x > 0; x--)
            {
                for (var y = 0; y < CartesianYRes; y++)
                {
                    thetha = (180 / Math.PI) * Math.Atan2(x, y);
                    r = Math.Sqrt(x * x + y * y);
                    r = r / (float)CartesianYRes * (MaxDistance - MinDistance);

                    if (thetha <= (HorizontalFieldOfView / 2) && r <= MaxDistance && r >= MinDistance)
                    {
                        xCoordinate = (int)Math.Round(((HorizontalFieldOfView / 2) - thetha) / HorizontalFieldOfView * WidthRes);
                        yCoordinate = (int)Math.Round(r / (MaxDistance - MinDistance) * imageHeight);
                        pixel = sonarImage.GetPixel(xCoordinate, yCoordinate);
                        sonarCartesianImage.SetPixel(CartesianXRes / 2 - x, y, pixel);
                    }
                    else
                    {
                        pixel = new Color(1, 1, 1, 1);
                        sonarCartesianImage.SetPixel(CartesianXRes / 2 - x, y, pixel);
                    }
                }

            }

            //populate right side of the swath
            for (var x = 0; x < CartesianXRes / 2; x++)
            {
                for (var y = 0; y < CartesianYRes; y++)
                {
                    thetha = (180 / Math.PI) * Math.Atan2(x, y);
                    r = Math.Sqrt(x * x + y * y);
                    r = r / CartesianYRes * (MaxDistance - MinDistance);

                    if (thetha <= (HorizontalFieldOfView / 2) && r <= MaxDistance && r >= MinDistance)
                    {
                        xCoordinate = (int)Math.Round((thetha + (HorizontalFieldOfView / 2)) / HorizontalFieldOfView * WidthRes);
                        yCoordinate = (int)Math.Round(r / (MaxDistance - MinDistance) * imageHeight);
                        pixel = sonarImage.GetPixel(xCoordinate, yCoordinate);
                        sonarCartesianImage.SetPixel(x + CartesianXRes / 2, y, pixel);
                    }
                    else
                    {
                        pixel = new Color(1, 1, 1, 1);
                        sonarCartesianImage.SetPixel(x + CartesianXRes / 2, y, pixel);
                    }
                }
            }

            sonarCartesianImage.Apply();
            byte[] bytes = sonarCartesianImage.EncodeToPNG();
            var dirPath = Application.dataPath + "/../SaveImages/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
            sonarCartesianDisplay.texture = sonarCartesianImage;
        }

    }

}