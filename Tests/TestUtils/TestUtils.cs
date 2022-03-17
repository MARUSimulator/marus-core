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

using System.Reflection;
using UnityEngine;

namespace TestUtils
{
    public static class Utils
    {
        public static void CallFixedUpdate<T>(T script)
        {
            CallNonpublicMethod(script, "FixedUpdate");
        }

        public static void CallUpdate<T>(T script)
        {
            CallNonpublicMethod(script, "Update");
        }

        public static void CallOnTriggerEnter<T>(T script, object parameter)
        {
            CallNonpublicMethod(script, "OnTriggerEnter", new object[] { parameter });
        }

        public static void CallAwake<T>(T script)
        {
            CallNonpublicMethod(script, "Awake");
        }

        public static void CallStart<T>(T script)
        {
            CallNonpublicMethod(script, "Start");
        }

        public static object CallNonpublicMethod<T>(T script, string methodName, object[] parameters=null) 
        {
            var typ = typeof(T);
            var method = typ.GetMethod(methodName);
            if (method == null)
            {
                method = typ.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (method != null)
            {
                return method.Invoke(script, parameters);
            }
            return false;
        }

        public static bool SetNonpublicField<T>(T script, string fieldName, object fieldValue=null, 
            bool isStatic=false) 
        {
            var typ = typeof(T);
            var field = typ.GetField(fieldName);
            if (field == null)
            {
                if (isStatic)
                    field = typ.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                else
                    field = typ.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (field != null)
            {
                field.SetValue(script, fieldValue);
                return true;
            }
            return false;
        }

        public static object GetNonpublicField<T>(T script, string fieldName, bool isStatic=false) 
        {
            var typ = typeof(T);
            var field = typ.GetField(fieldName);
            if (field == null)
            {
                if (isStatic)
                    field = typ.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                else
                    field = typ.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (field != null)
            {
                return field.GetValue(script);
            }
            return null;
        }

        public static T CreateAndInitializeObject<T>(string name, PrimitiveType? primitive=null) where T : MonoBehaviour
        {
            GameObject gameObject;
            if (primitive == null)
            {
                gameObject = new GameObject();
            }
            else
            {
                gameObject = GameObject.CreatePrimitive(primitive.Value);
            }
            gameObject.name = name;
            return InitializeWithScript<T>(gameObject);
        }

        public static T InitializeWithScript<T>(GameObject obj) where T : MonoBehaviour
        {
            var script = obj.AddComponent<T>();
            CallAwake(script);
            CallStart(script);
            return script;
        }

        public static void CreateEmptyScene()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
        }

        public static void CallPhysicsUpdate()
        {
            Physics.autoSimulation = false;
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.autoSimulation = true;
        }
    }
}
