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

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;

namespace Gemini.EMRS.Core { 

/// <summary>
/// Will be removed when Radar and IR are done
/// </summary>
public class Helper
    {

        public static T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                BinaryFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        public static GameObject CreateChildComponent<T>(Transform parentTransform, string childName) where T: MonoBehaviour
        {

            GameObject obj = new GameObject();
            obj.name = childName;
            obj.transform.SetParent(parentTransform);
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.transform.localPosition = new Vector3(0, 0, 0);
            obj.layer = parentTransform.gameObject.layer;

            obj.AddComponent<T>();
            return obj;
        }

        public static void PrintPartialByteArrayAs<T>(byte[] byteArray, int startIndex, int nrOfElements, string DebugTag = "Byte Array") where T: IConvertible
        {
            string stringArray = "";
            if (typeof(T) == typeof(float))
            {
                for (int i = 0; i < nrOfElements; i++)
                {
                    stringArray += "-" + System.BitConverter.ToSingle(byteArray, startIndex + i * sizeof(float)).ToString();
                }
            }
            else if (typeof(T) == typeof(byte))
            {
                stringArray += "-" + System.BitConverter.ToString(byteArray, startIndex, nrOfElements);
            }
            else
            {
                throw new Exception("Printing byte array of type: '" + typeof(T).ToString() + "' are not supported");
            }
            Debug.Log(DebugTag + " | " + stringArray);
            Debug.Log(DebugTag + " | " + "CPU is little endian: " + System.BitConverter.IsLittleEndian.ToString());
        }

    }
}