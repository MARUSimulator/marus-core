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
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Marus.Utils
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
            //var component = tf.GetComponent<Rigidbody>();
            if (tf == null || IsVehicle(tf))
            {
                return tf;
            }
            else
            {
                return GetVehicle(tf.parent);
                // var b = Utils.Helpers.GetParentRigidBody(tf, IsVehicle);
                // if (b != null)
                // {
                //     return b.transform;
                // }
                // return null;
            }
        }

        public static T GetComponentInParents <T>(GameObject startObject) where T : Component
        {
            T returnObject = null;
            T next_parent = null;
            GameObject currentObject = startObject;
            while(next_parent = currentObject?.GetComponentInParent<T>())
            {
                returnObject = next_parent;
                currentObject = next_parent.transform.parent?.gameObject;
            }
            return returnObject;
        }

        public static GameObject FindGameObjectInChildren(string name, GameObject parent, bool includeInactive=true)
        {
            Transform[] allChildren = parent.GetComponentsInChildren<Transform>(includeInactive);
            var obj = allChildren.FirstOrDefault(x => x.name == name);
            return (obj != null) ? obj.gameObject : null;
        }

        public static GameObject[] FindGameObjectsInLayerMask(LayerMask layers)
        {
            var goArray = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            var goList = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < goArray.Length; i++)
            {
                if ( ((1 << goArray[i].layer) & layers.value) > 0 )
                {
                    goList.Add(goArray[i]);
                }
            }
            if (goList.Count == 0)
            {
                return new GameObject[0];
            }
            return goList.ToArray();
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

        public static int PointFieldDataTypeToBytes(Sensor.PointField.Types.DataType type)
        {
            string name = Enum.GetName(typeof(Sensor.PointField.Types.DataType), type);
            int bytes = 0;
            var parsed = Int32.TryParse(name.Substring(name.Length - 2), out bytes);
            if (!parsed)
            {
                parsed = Int32.TryParse(name.Substring(name.Length - 1), out bytes);
            }
            return bytes / 8;
        }
    }

#if UNITY_EDITOR
    public static class EditorExtensions
    {
        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes a field by name.
        /// </summary>
        /// <param name="fieldToSkip">The name of the field that should be excluded. Example: "m_Script" will skip the default Script field.</param>
        public static void DrawInspectorExcept(this SerializedObject serializedObject, string fieldToSkip)
        {
            serializedObject.DrawInspectorExcept(new string[1] { fieldToSkip });
        }

        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes the specified fields by name.
        /// </summary>
        /// <param name="fieldsToSkip">
        /// An array of names that should be excluded.
        /// Example: new string[] { "m_Script" , "myInt" } will skip the default Script field and the Integer field myInt.
        /// </param>
        public static void DrawInspectorExcept(this SerializedObject serializedObject, string[] fieldsToSkip)
        {
            serializedObject.Update();
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Any(prop.name.Contains))
                        continue;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                }
                while (prop.NextVisible(false));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
