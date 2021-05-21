using UnityEngine;
using System.Collections;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Labust.Sensors.Core { 
    public class UnifiedArray<T> where T : struct
    {
        public NativeArray<T> nativeArray;
        public ComputeBuffer buffer;
        public T[] array;
        public string bufferName;
        public UnifiedArray(int numberOfElements, int elementSizeBytes, string gpuBufferName)
        {
            ComputeBuffer dataBuffer = new ComputeBuffer(numberOfElements, elementSizeBytes);
            NativeArray<T> dataArray;
            var VarType = typeof(T);
            if (VarType == typeof(byte))
            {
                dataArray = new NativeArray<T>(numberOfElements * elementSizeBytes, Allocator.Temp, NativeArrayOptions.ClearMemory);
            }
            else
            {
                dataArray = new NativeArray<T>(numberOfElements, Allocator.Temp, NativeArrayOptions.ClearMemory);
            }
            dataBuffer.SetData(dataArray);
            bufferName = gpuBufferName;
            nativeArray = dataArray;
            array = nativeArray.ToArray();
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
            nativeArray.Dispose();
        }
        public T[] AsynchUpdate(AsyncGPUReadbackRequest request)
        {
            nativeArray = request.GetData<T>();
            array = nativeArray.ToArray();
            return array;
        }
        public UnifiedArray<T> SynchUpdate(ComputeShader shader, string kernelName)
        {
            int kernelHandle = shader.FindKernel(kernelName);
            int Batch = (int)Mathf.Ceil((float)array.Length / 1024.0f);
            shader.Dispatch(kernelHandle, Batch, 1, 1);
            buffer.GetData(array);
            return this;
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