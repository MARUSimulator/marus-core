
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Labust.Utils
{
    public static class Helpers
    {
        /// <summary>
        /// Return RigidBody component in the first ancestor GameObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Rigidbody GetParentRigidBody(Transform obj, Func<Rigidbody, bool> where = null)
        {
            if(obj.TryGetComponent<Rigidbody>(out var rb))
            {
                if (where == null || where(rb))
                {
                    return rb;
                }
            }
            if (obj.parent != null)
                return GetParentRigidBody(obj.parent, where);
            else
                return null;
        }
        public static bool IsVehicle(Rigidbody rb)
        {
            return rb.tag == "Vehicle";
        }

        public static bool IsVehicle(Transform rb)
        {
            return rb.tag == "Vehicle";
        }

        public static Transform GetVehicle(Transform tf)
        {
            var component = tf.GetComponent<Rigidbody>();
            if (component != null && IsVehicle(component))
            {
                return component.transform;
            }
            else
            {
                var b = Utils.Helpers.GetParentRigidBody(tf, IsVehicle);
                if (b != null)
                {
                    return b.transform;
                }
                return null;
            }
        }

        public static GameObject FindGameObjectInChildren(string name, GameObject parent, bool includeInactive=true)
        {
            Transform[] allChildren = parent.GetComponentsInChildren<Transform>(includeInactive);
            var obj = allChildren.FirstOrDefault(x => x.name == name);
            return (obj != null) ? obj.gameObject : null;
        }

        public static Vector3 GetObjectScale(Transform t, bool includeSelf=true)
        {
            Vector3 currScale = (includeSelf) ? t.localScale
                : new Vector3(1, 1, 1);

            t = t.parent;
            while (t != null)
            {
                currScale = new Vector3(currScale.x * t.localScale.x, currScale.y * t.localScale.y, currScale.z * t.localScale.z);
                t = t.parent;
            }
            return currScale;
        }

        public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }

        public static Quaternion Round(this Quaternion quat, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Quaternion(
                Mathf.Round(quat.x * multiplier) / multiplier,
                Mathf.Round(quat.y * multiplier) / multiplier,
                Mathf.Round(quat.z * multiplier) / multiplier,
                Mathf.Round(quat.w * multiplier) / multiplier);
        }

        public static IEnumerable<TReturn> ZipWhereNotNull<TSource, TSecond, TReturn>(
            this IEnumerable<TSource> src, IEnumerable<TSecond> second, Func<TSource, TSecond, TReturn?> func)
            where TReturn : struct
        {
            var firstEnumerator = src.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();
            while (firstEnumerator.MoveNext())
            {
                if (secondEnumerator.MoveNext())
                {
                    var result = func(firstEnumerator.Current, secondEnumerator.Current);
                    if (result.HasValue)
                    {
                        yield return result.Value;
                    }
                }
                else
                {
                    secondEnumerator.Dispose();
                    break;
                }
            }
        }

        public static float RandomGaussian(float mean = 0.0f, float sigma = 1.0f)
        {
            float u, v, S;
        
            do
            {
                u = 2.0f * UnityEngine.Random.value - 1.0f;
                v = 2.0f * UnityEngine.Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);
        
            // Standard Normal Distribution
            float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        
            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            var d = 1.5f * sigma;
            float maxValue = mean + d;
            float minValue = mean - d;
            return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
    }
}
