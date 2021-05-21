using System;
using System.ComponentModel;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Serialization;

namespace Simulator.Sensors
{
    public class GeographicFrame : MonoBehaviour
    {
        [Header("Tangent plane origin")] 
        public GeoPoint origin;
        public double[] originEcef = new double[3];

        [Header("Debug")] 
        public double[] ecef = new double[3];

        ///Semi-major axis of the local geodetic datum ellipsoid
        const double a = 6378137;
        ///Flattening of the local geodetic datum ellipsoid
        const double f = 1/298.257223563;
        ///Semi-minor axis of the local geodetic datum ellipsoid
        const double b = a*(1-f);
        ///First eccentricity squared
        const double esq = 2*f-f*f;
        ///Secondary eccentricity squared
        const double ecsq = (a*a - b*b)/(b*b);
        
        public GeoPoint Unity2Geo(Vector3 position)
        {
            // var enu = position.Unity2Ros();
            ecef = enu2ECEF(position);

            return ecef2Geodetic(ecef);
        }

        private double[] enu2ECEF(Vector3 enu)
        {
            var slon = (float)Math.Sin(Math.PI * origin.longitude / 180.0);
            var slat = (float)Math.Sin(Math.PI*origin.latitude/180.0);
            var clon = (float)Math.Cos(Math.PI*origin.longitude/180.0);
            var clat = (float)Math.Cos(Math.PI*origin.latitude/180.0);
            
            return new double[3]{
                originEcef[0] - slon*enu.x - slat*clon*enu.y + clat*clon*enu.z,
                originEcef[1] + clon*enu.x - slat*slon*enu.y + clat*slon*enu.z,
                originEcef[2] + 0          +      clat*enu.y +      slat*enu.z
            };
        }

        private GeoPoint ecef2Geodetic(double[] ecef)
        {
            double x = ecef[0];
            double y = ecef[1]; 
            double z = ecef[2];
            
            double r = Math.Sqrt(x*x + y*y);
            double F = 54*(b*b)*(z*z);
            double G = r*r + (1-esq)*z*z - esq*(a*a - b*b);
            double c = esq*esq*F*r*r/(G*G*G);
            double s = cbrt(1+c+Math.Sqrt(c*c+2*c));
            double P = F/(3*(s+1/s+1)*(s+1/s+1)*G*G);
            double Q = Math.Sqrt(1+2*esq*esq*P);
            double r0 = -P*esq*r/(1+Q) + Math.Sqrt(0.5*a*a*(1+1/Q)
                                              - P*(1-esq)*z*z/(Q*(1+Q)) - 0.5*P*r*r);
            double re = r-esq*r0;
            double U = Math.Sqrt(re*re + z*z);
            double V = Math.Sqrt(re*re + (1-esq)*z*z);
            double z0 = b*b*z/(a*V);

            return new GeoPoint(Math.Atan((z+ecsq*z0)/r)*180/Math.PI,Math.Atan2(y,x)*180/Math.PI,
                U*(1-b*b/(a*V)));
        }

        private double cbrt(double root)
        {
            if (root < 0)
                return -Math.Pow(-root, 1d / 3d);

            return Math.Pow(root, 1d / 3d);
        }

        private double[] geodetic2ecef(GeoPoint geo)
        {
            double rlat = geo.latitude*Math.PI/180;
            double rlon = geo.longitude*Math.PI/180;
            double R = a/Math.Sqrt(1-esq*Math.Sin(rlat)*Math.Sin(rlat));

            return new double[3]{((R+geo.altitude)*Math.Cos(rlat)*Math.Cos(rlon)),
                ((R+geo.altitude)*Math.Cos(rlat)*Math.Sin(rlon)),
                ((R+geo.altitude - esq*R)*Math.Sin(rlat))};
        }
        
        private void Start()
        {
            originEcef = geodetic2ecef(origin);
        }
    }
}