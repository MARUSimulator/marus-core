// Copyright 2021 Laboratory for Underwater Systems and Technologies (LABUST)
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

using Unity.Collections;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Linq;

namespace Marus.PcdUtils
{
    public static class PCDUtils
    {

        private static string GenerateMetadata(int numOfPoints, string format)
        {
            return
                "VERSION .7\n" +
                "FIELDS x y z\n" +
                "SIZE 4 4 4\n" +
                "TYPE F F F\n" +
                "COUNT 1 1 1\n" +
                "WIDTH " + numOfPoints.ToString() + "\n" +
                "HEIGHT 1\n" +
                "VIEWPOINT 0 0 0 1 0 0 0\n" +
                "POINTS " + numOfPoints.ToString() + "\n" +
                "DATA " + format + "\n";
        }

        public static void WriteToPcdFile(string filePath, NativeArray<Vector3> pointcloud, string format)
        {
            var _pointsFiltered = pointcloud.Where(x => x != Vector3.zero).ToArray();
            var byteSize = _pointsFiltered.Length * 12;
            if (_pointsFiltered.Length == 0)
            {
                return;
            }

            string metadata = GenerateMetadata(_pointsFiltered.Length, format);
            if (format == "binary")
            {
                File.WriteAllText(filePath, metadata);
                unsafe
                {
                    fixed (Vector3* p = &_pointsFiltered[0])
                    {
                        UnmanagedMemoryStream sourceStream = new UnmanagedMemoryStream((byte*)p, byteSize, byteSize, FileAccess.ReadWrite);
                        using (var fs = new FileStream(filePath, FileMode.Append))
                        {
                            sourceStream.CopyTo(fs);
                        }
                        sourceStream.Close();
                    }
                }
            }
            else if (format == "binary_compressed")
            {
                File.WriteAllText(filePath, metadata);
                unsafe
                {
                    fixed (Vector3* p = &_pointsFiltered[0])
                    {
                        TransposeArrayInPlace((float*)p, _pointsFiltered.Length, 3);
                        var inputBuffer = (byte*)p;

                        byte[] outputBuffer = null;
                        int compressedSize = CLZF2.Compress(inputBuffer, ref outputBuffer, byteSize);

                        byte[] compressedSizeBytes = BitConverter.GetBytes(compressedSize);
                        byte[] uncompressedSizeBytes = BitConverter.GetBytes(byteSize);

                        using (var stream = new FileStream(filePath, FileMode.Append))
                        {
                            stream.Write(compressedSizeBytes, 0, compressedSizeBytes.Length);
                            stream.Write(uncompressedSizeBytes, 0, uncompressedSizeBytes.Length);
                            stream.Write(outputBuffer, 0, compressedSize);
                        }
                    }
                }
            }
            else if (format == "ascii")
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    writer.Write(metadata);
                    foreach (var point in _pointsFiltered)
                    {
                        var p = string.Format("{0} {1} {2}", point.x.ToString("G9"), point.y.ToString("G9"), point.z.ToString("G9"));
                        writer.WriteLine(p);
                    }
                }
            }
        }

        // Non-square matrix transpose of matrix of size r x c and base address A
        unsafe private static void TransposeArrayInPlace(float* A, int r, int c)
        {
            int size = r*c - 1;
            int t_idx; // holds element to be replaced, eventually becomes next element to move
            int next; // location of 't' to be moved
            int cycleBegin; // holds start of cycle
            int i; // iterator
            var b = new bool[size + 1]; // hash to mark moved elements

            b[0] = b[size] = true;
            i = 1; // Note that A[0] and A[size-1] won't move
            while (i < size)
            {
                cycleBegin = i;
                t_idx = i;
                do
                {
                    // Input matrix [r x c]
                    // Output matrix
                    // i_new = (i*r)%(N-1)
                    next = (i * r) % size;
                    var temp = A[next];
                    A[next] = A[t_idx];
                    A[t_idx] = temp;
                    b[i] = true;
                    i = next;
                }
                while (i != cycleBegin);
                // Get Next Move (what about querying random location?)
                for (i = 1; i < size && b[i]; i++);
            }
        }
    }
}
