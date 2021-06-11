using UnityEngine;
using System.Collections;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Labust.Sensors.Core { 

    public class ComputeBufferDataExtractor<T> where T : struct
    {
        public ComputeBuffer buffer;
        public string bufferName;
        public T[] data;

        public ComputeBufferDataExtractor(int numElements, int elementSizeBytes, string gpuBufferName)
        {
            ComputeBuffer dataBuffer = new ComputeBuffer(numElements, elementSizeBytes);
            var VarType = typeof(T);
            if (VarType == typeof(byte))
            {
                data = new T[numElements * elementSizeBytes];
            }
            else
            {
                data = new T[numElements];
            }
            dataBuffer.SetData(data);
            bufferName = gpuBufferName;
            buffer = dataBuffer;
        }
        public void SetBuffer(ComputeShader shader, string kernelName)
        {
            int kernelHandle = shader.FindKernel(kernelName);
            shader.SetBuffer(kernelHandle, bufferName, buffer);
        }
        public void Delete()
        {
            buffer.Release();
            buffer.Dispose();
        }
        public T[] AsynchUpdate(AsyncGPUReadbackRequest request)
        {
            data = request.GetData<T>().ToArray();
            return data;
        }
        public T[] SynchUpdate(ComputeShader shader, string kernelName)
        {
            int kernelHandle = shader.FindKernel(kernelName);
            int Batch = (int)Mathf.Ceil((float)data.Length / 1024.0f);
            shader.Dispatch(kernelHandle, Batch, 1, 1);
            buffer.GetData(data);
            return data;
        }
    }
    /*
        private struct UnifiedCompute
        {
            private UnifiedArray<T>[] _unifiedArrays;
            private ComputeShader _kernelShader;
            private string _kernelName;
            private int _kernelHandle;
            public UnifiedCompute(ComputeShader kernelShader, string kernelName, params UnifiedArray<T>[] unifiedArrays)
            {
                _unifiedArrays = unifiedArrays;
                _kernelShader = kernelShader;
                _kernelName = kernelName;
                _kernelHandle = _kernelShader.FindKernel(_kernelName);
                for (int i = 0; i < _unifiedArrays.Length; i++)
                {
                    _kernelShader.SetBuffer(_kernelHandle, _unifiedArrays[i].bufferName, _unifiedArrays[i].buffer);
                }

            }
            private void UpdateBuffers()
            {
                _kernelShader.Dispatch(_kernelHandle, (int)Mathf.Ceil((float)_unifiedArrays.Length / 1024.0f), 1, 1);
            }
            private int GetNativeArray(int index)
            {
                var request = AsyncGPUReadback.Request(_unifiedArrays[index].buffer);
                //yield return new WaitUntil(() => request2.done);
                //lidarDataByte.nativeArray = request2.GetData<byte>();
                //byte[] LidarByteArray = lidarDataByte.nativeArray.ToArray();
                return 1;
            }
        }
        */
}