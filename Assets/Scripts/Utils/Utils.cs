
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



    }
}