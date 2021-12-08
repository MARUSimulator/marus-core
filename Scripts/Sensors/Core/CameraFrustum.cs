using UnityEngine;
using System.Collections;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

using Unity.Collections;
using UnityEngine.Rendering;


namespace Labust.Sensors.Core
{
    public class CameraFrustum
    {
        public int pixelWidth { get; }
        public int pixelHeight { get; }
        public float farPlane { get; }
        public float nearPlane { get; }
        public float verticalAngle { get; }
        public float horisontalAngle { get; }
        public float aspectRatio { get; }
        public float verticalSideAngles { get; }
        public Matrix4x4 cameraMatrix { get; }

        public CameraFrustum(int pixelWidth, int pixelHeight, float farPlane, float nearPlane, float verticalAngle)
        {
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
            this.farPlane = farPlane;
            this.nearPlane = nearPlane;
            this.verticalAngle = verticalAngle;

            aspectRatio = (float)this.pixelWidth / (float)this.pixelHeight;
            horisontalAngle = 2 * Mathf.Atan(aspectRatio * Mathf.Tan(this.verticalAngle / 2));
            verticalSideAngles = 2 * Mathf.Atan(Mathf.Cos(horisontalAngle / 2) * Mathf.Tan(this.verticalAngle / 2));
            cameraMatrix = MakeCameraMatrix(aspectRatio, this.verticalAngle, farPlane, nearPlane);
        }

        public CameraFrustum(int pixelWidth, int pixelHeight, float farPlane, float nearPlane, float focalLengthMilliMeters, float pixelSizeInMicroMeters)
        {
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
            this.farPlane = farPlane;
            this.nearPlane = nearPlane;
            verticalAngle = 2 * Mathf.Atan((float)pixelHeight*pixelSizeInMicroMeters*Mathf.Pow(10,-3)/(2 * focalLengthMilliMeters));

            aspectRatio = (float)this.pixelWidth / (float)this.pixelHeight;
            horisontalAngle = 2 * Mathf.Atan(aspectRatio * Mathf.Tan(verticalAngle / 2));
            verticalSideAngles = 2 * Mathf.Atan(Mathf.Cos(horisontalAngle / 2) * Mathf.Tan(verticalAngle / 2));
            cameraMatrix = MakeCameraMatrix(aspectRatio, verticalAngle, farPlane, nearPlane);
        }

        // Verified
        public CameraFrustum(int pixelWidth, float farPlane, float nearPlane, float horisontalAngle, float verticalAngle)
        {
            this.horisontalAngle = horisontalAngle;
            this.verticalAngle = verticalAngle;
            this.farPlane = farPlane;
            this.nearPlane = nearPlane;
            this.pixelWidth = pixelWidth;

            verticalSideAngles = 2 * Mathf.Atan(Mathf.Cos(horisontalAngle / 2) * Mathf.Tan(this.verticalAngle / 2));
            aspectRatio = Mathf.Tan(this.horisontalAngle / 2) / Mathf.Tan(verticalAngle / 2);
            pixelHeight = (int)((float)pixelWidth / aspectRatio);
            cameraMatrix = MakeCameraMatrix(aspectRatio, verticalAngle, farPlane, nearPlane);
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
