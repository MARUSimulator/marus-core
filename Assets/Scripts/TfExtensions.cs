using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Labust.Core
{

    public static class TfExtensions
    {
        public static Vector3 Standard2Unity(this Vector3 vector3)
        {
            return new Vector3(vector3.x, vector3.z, vector3.y);
        }

        public static Vector3 Unity2Standard(this Vector3 vector3)
        {
            return new Vector3(vector3.x, vector3.z, vector3.y);
        }

        public static Quaternion Standard2Unity(this Quaternion quaternion)
        {
            return new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
        } 

        public static Quaternion Unity2Standard(this Quaternion quaternion)
        {
            return new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
        }
    }

}