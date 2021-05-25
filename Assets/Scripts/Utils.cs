



using UnityEngine;

namespace Utils
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
    }
}