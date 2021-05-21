using UnityEngine;
using System.Collections;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

using Unity.Collections;
using UnityEngine.Rendering;


namespace Labust.Sensors.Core.ZBuffer{
    public class CameraFrustum
    {
        public int _pixelWidth { get; }
        public int _pixelHeight { get; }
        public float _farPlane { get; }
        public float _nearPlane { get; }
        public float _verticalAngle { get; }
        public float _horisontalAngle { get; }
        public float _aspectRatio { get; }
        public float _verticalSideAngles { get; }
        public Matrix4x4 _cameraMatrix { get; }
        public CameraFrustum(int pixelWidth, int pixelHeight, float farPlane, float nearPlane, float verticalAngle)
        {
            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;
            _farPlane = farPlane;
            _nearPlane = nearPlane;
            _verticalAngle = verticalAngle;

            _aspectRatio = (float)_pixelWidth / (float)_pixelHeight;
            _horisontalAngle = 2 * Mathf.Atan(_aspectRatio * Mathf.Tan(_verticalAngle / 2));
            _verticalSideAngles = 2 * Mathf.Atan(Mathf.Cos(_horisontalAngle / 2) * Mathf.Tan(_verticalAngle / 2));
            _cameraMatrix = new Matrix4x4();
            _cameraMatrix = MakeCameraMatrix(_aspectRatio, _verticalAngle, farPlane, nearPlane);
        }

        public CameraFrustum(int pixelWidth, int pixelHeight, float farPlane, float nearPlane, float focalLengthMilliMeters, float pixelSizeInMicroMeters)
        {
            _pixelWidth = pixelWidth;
            _pixelHeight = pixelHeight;
            _farPlane = farPlane;
            _nearPlane = nearPlane;
            _verticalAngle = 2 * Mathf.Atan((float)pixelHeight*pixelSizeInMicroMeters*Mathf.Pow(10,-3)/(2 * focalLengthMilliMeters));
            Debug.Log("_VerticalAngle" + _verticalAngle.ToString() + " power " + ((float)pixelHeight).ToString());

            _aspectRatio = (float)_pixelWidth / (float)_pixelHeight;
            _horisontalAngle = 2 * Mathf.Atan(_aspectRatio * Mathf.Tan(_verticalAngle / 2));
            _verticalSideAngles = 2 * Mathf.Atan(Mathf.Cos(_horisontalAngle / 2) * Mathf.Tan(_verticalAngle / 2));
            _cameraMatrix = new Matrix4x4();
            _cameraMatrix = MakeCameraMatrix(_aspectRatio, _verticalAngle, farPlane, nearPlane);
        }

        // Verified
        public CameraFrustum(int pixelWidth, float farPlane, float nearPlane, float horisontalAngle, float verticalSideAngles)
        {
            _horisontalAngle = horisontalAngle;
            _verticalSideAngles = verticalSideAngles;
            _farPlane = farPlane;
            _nearPlane = nearPlane;
            _pixelWidth = pixelWidth;

            _verticalAngle = 2 * Mathf.Atan(Mathf.Tan(_verticalSideAngles / 2) / Mathf.Cos(_horisontalAngle / 2));
            _aspectRatio = Mathf.Tan(_horisontalAngle / 2) / Mathf.Tan(_verticalAngle / 2);
            _pixelHeight = (int)((float)pixelWidth / _aspectRatio);
            _cameraMatrix = new Matrix4x4();
            _cameraMatrix = MakeCameraMatrix(_aspectRatio, _verticalAngle, farPlane, nearPlane);
        }
        public CameraFrustum(float imageMemorySize, DepthCameras.BufferPrecision depthPrecision, float farPlane, float nearPlane, float horisontalAngle, float verticalSideAngles)
        {
            _horisontalAngle = horisontalAngle;
            _verticalSideAngles = verticalSideAngles;
            _farPlane = farPlane;
            _nearPlane = nearPlane;

            _verticalAngle = 2 * Mathf.Atan(Mathf.Tan(_verticalSideAngles / 2) / Mathf.Cos(_horisontalAngle / 2));
            _aspectRatio = Mathf.Tan(_horisontalAngle / 2) / Mathf.Tan(_verticalAngle / 2);

            int precision = 0;
            if (depthPrecision == DepthCameras.BufferPrecision.bit16) { precision = 16; }
            if (depthPrecision == DepthCameras.BufferPrecision.bit24) { precision = 24; }
            if (depthPrecision == DepthCameras.BufferPrecision.bit32) { precision = 32; }
            _pixelHeight = (int)Mathf.Sqrt(8 * imageMemorySize / (_aspectRatio * precision));
            _pixelWidth = (int)(_pixelHeight * _aspectRatio);
            _cameraMatrix = new Matrix4x4();
            _cameraMatrix = MakeCameraMatrix(_aspectRatio, _verticalAngle, farPlane, nearPlane);
        }
        // Verified
        private Matrix4x4 MakeCameraMatrix(float a, float VFOV, float f, float n)
        {
            float P_2 = 1 / Mathf.Tan(VFOV / 2);
            float P_1 = P_2 / a;
            float P_3 = -(f + n) / (f - n);
            float P_4 = -2 * f * n / (f - n);

            Vector4 colum_1 = new Vector4(P_1, 0, 0, 0);
            Vector4 colum_2 = new Vector4(0, P_2, 0, 0);
            Vector4 colum_3 = new Vector4(0, 0, P_3, -1);
            Vector4 colum_4 = new Vector4(0, 0, P_4, 0);

            return new Matrix4x4(colum_1, colum_2, colum_3, colum_4);
        }
    }
}