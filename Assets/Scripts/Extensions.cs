using Sensorstreaming;
using Common;
using UnityEngine;
using mVector3 = Common.Vector3;
using uVector3 = UnityEngine.Vector3;

namespace Labust.Networking
{

    public static class MessageExtensions
    {

        public static mVector3 AsMsg(this uVector3 vec) =>
            new mVector3() { X = vec.x, Y = vec.y, Z = vec.z };

        public static uVector3 AsUnity(this mVector3 vec) =>
            new uVector3(vec.X, vec.Y, vec.Z);
    }
}