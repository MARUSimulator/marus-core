using System.Reflection;
using UnityEngine;

namespace UnitTests
{

    public static class TestUtils
    {
        public static void CallFixedUpdate<T>(T script)
        {
            CallNonpublicMethod(script, "FixedUpdate");
        }

        public static void CallAwake<T>(T script)
        {
            CallNonpublicMethod(script, "Awake");
        }

        public static void CallStart<T>(T script)
        {
            CallNonpublicMethod(script, "Start");
        }

        public static bool CallNonpublicMethod<T>(T script, string methodName, object[] parameters=null) 
        {
            var typ = typeof(T);
            var method = typ.GetMethod(methodName);
            if (method == null)
            {
                method = typ.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (method != null)
            {
                method.Invoke(script, parameters);
                return true;
            }
            return false;
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
    }
}