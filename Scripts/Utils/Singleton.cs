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

using UnityEngine;
using System.Reflection;

namespace Marus.Utils
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
                    }
                    var init = typeof(T).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
                    init.Invoke(instance, null);
                }
                return instance;
            }
        }

        protected virtual void Initialize()
        {
        }
    }
}
