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

using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Marus.Core
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
            var quat = new Quaternion(-quaternion.x, -quaternion.z, -quaternion.y, quaternion.w);
            // angle from front vector (z) to standardized east (x)
            return quat * Quaternion.Euler(0, 90, 0);
        }

        /// Use this conversion when translating global rotation from Unity to ROS Map (ENU) frame.
        public static Quaternion Unity2Map(this Quaternion quaternion)
        {
            // angle from standardized east (x) to front vector (z)
            var quat = quaternion * Quaternion.Euler(0, -90, 0);
            return new Quaternion(-quat.x, -quat.z, -quat.y, quat.w);
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