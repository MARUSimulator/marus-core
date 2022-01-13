
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
        public static Rigidbody GetParentRigidBody(Transform obj)
        {
            if(obj.TryGetComponent<Rigidbody>(out var rb))
            {
                return rb;
            }
            if (obj.parent != null)
                return GetParentRigidBody(obj.parent);
            else
                return null;
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
    }
}
