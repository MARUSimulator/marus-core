using System;
using UnityEngine;

namespace Labust.Core
{
    //TODO
    [System.Serializable]
    public struct GeoPoint
    {
        [Header("Tangent plane origin")] 

        public float latitude;
        public float longitude;
        public float altitude;

        ///Semi-major axis of the local geodetic datum ellipsoid
        const double a = 6378137;
        ///Flattening of the local geodetic datum ellipsoid
        const double f = 1 / 298.257223563;
        ///Semi-minor axis of the local geodetic datum ellipsoid
        const double b = a * (1 - f);
        ///First eccentricity squared
        const double esq = 2 * f - f * f;
        ///Secondary eccentricity squared
        const double ecsq = (a * a - b * b) / (b * b);

        public GeoPoint(float latitude, float longitude, float altitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
        }

        public GeoPoint(Vector3 ecef)
        {
            double x = ecef.x;
            double y = ecef.y; 
            double z = ecef.z;

            double r = Math.Sqrt(x * x + y * y);
            double F = 54 * (b*b) * (z*z);
            double G = r * r + (1 - esq)* z * z - esq * (a * a - b * b);
            double c = esq * esq * F * r * r / (G * G *G);
            double s = cbrt(1 + c + Math.Sqrt(c * c + 2 * c));
            double P = F / (3 * (s + 1 / s + 1) * ( s + 1 / s + 1) * G * G);
            double Q = Math.Sqrt(1 + 2 * esq * esq * P);
            double r0 = -P * esq * r / (1 + Q) + Math.Sqrt(0.5 * a * a * ( 1 + 1 / Q)
                                              - P * (1 - esq) * z * z / (Q * (1 + Q)) - 0.5 * P * r * r);
            double re = r - esq * r0;
            double U = Math.Sqrt(re * re + z * z);
            double V = Math.Sqrt(re * re + (1 - esq) * z *z);
            double z0 = b * b *z / (a * V);
            this.latitude = (float)(Math.Atan((z + ecsq * z0) / r) * 180 / Math.PI);
            this.longitude = (float)(Math.Atan2(y, x) * 180 / Math.PI);
            this.altitude = (float)(U * (1 - b * b / (a * V)));
        }

        public static GeoPoint Ecef2Geodetic(Vector3 ecef)
        {
            double x = ecef.x;
            double y = ecef.y; 
            double z = ecef.z;
            
            double r = Math.Sqrt(x * x + y * y);
            double F = 54 * (b*b) * (z*z);
            double G = r * r + (1 - esq)* z * z - esq * (a * a - b * b);
            double c = esq * esq * F * r * r / (G * G *G);
            double s = cbrt(1 + c + Math.Sqrt(c * c + 2 * c));
            double P = F / (3 * (s + 1 / s + 1) * ( s + 1 / s + 1) * G * G);
            double Q = Math.Sqrt(1 + 2 * esq * esq * P);
            double r0 = -P * esq * r / (1 + Q) + Math.Sqrt(0.5 * a * a * ( 1 + 1 / Q)
                                              - P * (1 - esq) * z * z / (Q * (1 + Q)) - 0.5 * P * r * r);
            double re = r - esq * r0;
            double U = Math.Sqrt(re * re + z * z);
            double V = Math.Sqrt(re * re + (1 - esq) * z *z);
            double z0 = b * b *z / (a * V);

            return new GeoPoint(
                (float)(Math.Atan((z + ecsq * z0) / r) * 180 / Math.PI), 
                (float)(Math.Atan2(y, x) * 180 / Math.PI),
                (float)(U * (1 - b * b / (a * V))));
        }

        private static double cbrt(double root)
        {
            if (root < 0)
                return -Math.Pow(-root, 1d / 3d);

            return Math.Pow(root, 1d / 3d);
        }

        public static Vector3 Geodetic2ecef(GeoPoint geo)
        {
            double rlat = geo.latitude * Math.PI / 180;
            double rlon = geo.longitude * Math.PI / 180;
            double R = a / Math.Sqrt(1 - esq * Math.Sin(rlat) * Math.Sin(rlat));

            return new Vector3(
                (float)((R + geo.altitude) * Math.Cos(rlat) * Math.Cos(rlon)),
                (float)((R + geo.altitude) * Math.Cos(rlat) * Math.Sin(rlon)),
                (float)((R + geo.altitude - esq * R) * Math.Sin(rlat)));
        }

        public static Vector3 Enu2ECEF(GeographicFrame frame, Vector3 enu)
        {
            var origin = frame.origin;
            var originEcef = frame.originEcef;
            var slon = (float)Math.Sin(Math.PI * origin.longitude / 180.0);
            var slat = (float)Math.Sin(Math.PI * origin.latitude / 180.0);
            var clon = (float)Math.Cos(Math.PI * origin.longitude / 180.0);
            var clat = (float)Math.Cos(Math.PI * origin.latitude / 180.0);
            
            return new Vector3(
                (float)(originEcef[0] - slon*enu.x - slat*clon*enu.y + clat*clon*enu.z),
                (float)(originEcef[1] + clon*enu.x - slat*slon*enu.y + clat*slon*enu.z),
                (float)(originEcef[2] + 0          +      clat*enu.y +      slat*enu.z)
            );
        }
    }
}