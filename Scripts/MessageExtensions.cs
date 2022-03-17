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

using mVector3 = Geometry.Vector3;
using uVector3 = UnityEngine.Vector3;
using mQuaternion = Geometry.Quaternion;
using uQuaternion = UnityEngine.Quaternion;
using mPoint = Geometry.Point;
using mColor = Std.ColorRGBA;
using UnityEngine;

namespace Labust.Networking
{
    public static class MessageExtensions
    {
        public static mVector3 AsMsg(this uVector3 vec) =>
            new mVector3() { X = vec.x, Y = vec.y, Z = vec.z };

        public static uVector3 AsUnity(this mVector3 vec) =>
            new uVector3((float)vec.X, (float)vec.Y, (float)vec.Z);

        public static mQuaternion AsMsg(this uQuaternion vec) =>
            new mQuaternion { X = vec.x, Y = vec.y, Z = vec.z, W = vec.w };

        public static uQuaternion AsUnity(this mQuaternion vec) =>
            new uQuaternion((float)vec.X, (float)vec.Y, (float)vec.Z, (float)vec.W);

        public static uVector3 AsUnity(this mPoint point) =>
            new uVector3((float)point.X, (float)point.Y, (float)point.Z);

        public static Color AsUnity(this mColor color) =>
            new Color(color.R, color.G, color.B, color.A);
    }
}
