using UnityEngine;
using System;
using System.Reflection;

namespace Labust.Utils
{

    /// <summary>
    /// This is generic singleton implementation.
    /// This is the best way to implement singleton in Unity, see <see cref="!:http://www.unitygeek.com/unity_c_singleton/">here.</see> 
    /// </summary> 
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
						if (Application.isPlaying) // DontDestroyOnLoad does not work outside PlayMode
						{
							DontDestroyOnLoad(instance.gameObject);
						}
						// call initialize
						var init = typeof(T).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
						init.Invoke(instance, null);
                    }
                }
                return instance;
            }
        }

        protected virtual void Initialize()
        {

        }
    }
}
