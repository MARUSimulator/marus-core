using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Labust.Core
{

    public static class TfExtensions
    {
        public static Vector3 Map2Unity(this Vector3 vector3)
        {
            return new Vector3(vector3.x, vector3.z, vector3.y);
        }
        
        /// Use this conversion when translating global coordinates from Unity to ROS Map (ENU) frame.
        public static Vector3 Unity2Map(this Vector3 vector3)
        {
            return new Vector3(vector3.x, vector3.z, vector3.y);
        }
        
        public static Quaternion Map2Unity(this Quaternion quaternion)
        {
            return new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
        } 

        /// Use this conversion when translating global rotation from Unity to ROS Map (ENU) frame.
        public static Quaternion Unity2Map(this Quaternion quaternion)
        {
            return new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
        }
        
        /// Use this conversion when translating local coordinates from Unity to ROS Forward-Left-Up body frames.
        public static Vector3 Unity2Body(this Vector3 vector3)
        {
            return new Vector3(vector3.z, -vector3.x, vector3.y);
        }
        
        /// Use this conversion when translating local rotations from Unity to ROS Forward-Left-Up body frames.
        public static Quaternion Unity2Body(this Quaternion quaternion)
        {
            return new Quaternion(-quaternion.z, quaternion.x, -quaternion.y, quaternion.w);
        }
    }

}