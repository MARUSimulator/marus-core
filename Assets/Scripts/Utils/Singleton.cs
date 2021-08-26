using UnityEngine;
using System;
namespace Labust.Utils
{

	/// <summary>
	/// This is generic singleton implementation.
	/// This is the best way to implement singleton in Unity, see <see cref="!:http://www.unitygeek.com/unity_c_singleton/">here.</see> 
	/// </summary> 
	public class Singleton<T> : MonoBehaviour where T : Component
	{
		private static T instance;
		public static T Instance {
			get {
				if (instance == null) {
					instance = FindObjectOfType<T> ();
					if (instance == null) {
						GameObject obj = new GameObject ();
						obj.name = typeof(T).Name;
						instance = obj.AddComponent<T>();
					}
				}
				return instance;
			}
		}
	
		public void Awake()
		{
			if (instance == null) {
				instance = this as T;
				DontDestroyOnLoad (this.gameObject);
			} else {
				Destroy (gameObject);
			}
			Initialize();
		}


		public virtual void Initialize()
		{

		}
	}
}
