
using System.Linq;
using UnityEngine;

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

        /// <summary>
        /// Samples from normal distribution N(0, 1)
        /// Using Marsaglia polar method
        /// <see cref="!:https://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/">Reference</see> 
        /// </summary>
        /// <returns></returns>
        public static float NextGaussian()
        {
            float v1, v2, s;
            do {
                v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
                v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0f || s == 0f);
            s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

            return v1 * s;
        }

        /// <summary>
        /// Uses NextGaussian method to sample from arbitrary distribution
        /// N(mean, stddev^2)
        /// </summary>
        /// <param name="mean">Mean value</param>
        /// <param name="standard_deviation">Standard deviation</param>
        /// <returns></returns>
        public static float NextGaussian(float mean, float standard_deviation)
        {
            return mean + NextGaussian() * standard_deviation;
        }
    }
}