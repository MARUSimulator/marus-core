using UnityEngine;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;

namespace Gemini.EMRS.Core { 
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