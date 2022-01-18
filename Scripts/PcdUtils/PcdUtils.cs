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
using System.Collections.Generic;

namespace Marus.Utils
{
    public static class PCDSaver
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

        /// <summary>
        /// Write pointcloud to .pcd file.
        /// </summary>
        /// <param name="filePath">Location for saving pcd file</param>
        /// <param name="pointcloud">Points data</param>
        /// <param name="format">Either ascii, binary or binary_compressed</param>
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

        /// <summary>
        /// Non-square matrix transpose of matrix of size r x c and base address A
        /// </summary>
        /// <param name="A">Pointer to flattened matrix</param>
        /// <param name="r">Row count</param>
        /// <param name="c">Column count</param>
        unsafe public static void TransposeArrayInPlace(float* A, int r, int c)
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

    /// <summary>
    /// Used for loading pointcloud data from pcd file.
    /// </summary>
    public static class PCDLoader
    {
        /// <summary>
        /// Load PointCloud object from pcd file
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns></returns>
        public static PointCloud LoadFromPcd(string path)
        {
            string format;
            List<(string, int)> fields;
            int numOfPoints;
            int headerOffset;
            (numOfPoints, format, fields, headerOffset) = ReadMetadataHeader(path);
            PointCloud p = new PointCloud();
            if (format == "binary_compressed")
            {
                p = ReadBinaryCompressedPoints(path, headerOffset, numOfPoints, fields);
            }
            else if (format == "binary")
            {
                p = ReadBinaryPoints(path, headerOffset, numOfPoints, fields);
            }
            else if (format == "ascii")
            {
                p = ReadAsciiPoints(path, headerOffset, numOfPoints, fields);
            }

            return p;
        }

        private static PointCloud ReadAsciiPoints(string path, int headerOffset, int numOfPoints, List<(string, int)> fields)
        {
            Vector3[] points = new Vector3[numOfPoints];
            Vector3[] normals = new Vector3[numOfPoints];
            Color[] colors = new Color[numOfPoints];
            float x = 0f;
            float y = 0f;
            float z = 0f;

            float normal_x = 0f;
            float normal_y = 0f;
            float normal_z = 0f;

            float r = 0f;
            float g = 1f;
            float b = 0f;
            float a = 1f;
            bool hasNormals = false;
            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(path))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.ASCII, false, BufferSize)) 
                {
                    string line = streamReader.ReadLine();
                    while (true)
                    {
                        if (line.ToLower().StartsWith("data")) break;
                        line = streamReader.ReadLine();
                    }
                    for (int i = 0; i < numOfPoints; i++)
                    {
                        line = streamReader.ReadLine();

                        string[] pointFields = line.Split(' ');

                        int j = 0;
                        foreach ((var field, var size) in fields)
                        {
                            switch (field.ToLower())
                            {
                                case "x":
                                {
                                    x = Single.Parse(pointFields[j]);
                                    break;
                                }
                                case "y":
                                {
                                    y = Single.Parse(pointFields[j]);
                                    break;
                                }
                                case "z":
                                {
                                    z = Single.Parse(pointFields[j]);
                                    break;
                                }
                                case "normal_x":
                                {
                                    normal_x = Single.Parse(pointFields[j]);
                                    break;
                                }
                                case "normal_y":
                                {
                                    normal_y = Single.Parse(pointFields[j]);
                                    break;
                                }
                                case "normal_z":
                                {
                                    hasNormals = true;
                                    normal_z = Single.Parse(pointFields[j]);
                                    break;
                                }
                                case "rgb":
                                {
                                    float rgb = Single.Parse(pointFields[j]);
                                    var bytes = BitConverter.GetBytes(rgb);
                                    r = (float) (bytes[0] / 255.0f);
                                    g = (float) (bytes[1] / 255.0f);
                                    b = (float) (bytes[2] / 255.0f);
                                    break;
                                }
                                case "rgba":
                                {
                                    float rgb = Single.Parse(pointFields[j]);
                                    var bytes = BitConverter.GetBytes(rgb);
                                    r = (float) (bytes[0] / 255.0f);
                                    g = (float) (bytes[1] / 255.0f);
                                    b = (float) (bytes[2] / 255.0f);
                                    b = (float) (bytes[3]);
                                    break;
                                }
                            }
                            j++;
                        }
                        points[i] = new Vector3(x, z, y);
                        normals[i] = new Vector3(normal_x, normal_z, normal_y);
                        colors[i] = new Color(b, g, r, a);
                    }
                }
            }
            PointCloud p = new PointCloud(points);
            if (hasNormals)
            {
                p.Normals = normals;
            }
            p.Colors = colors;
            return p;
        }

        private static PointCloud ReadBinaryPoints(string path, int headerOffset, int numOfPoints, List<(string, int)> fields)
        {
            Vector3[] points = new Vector3[numOfPoints];
            Color[] colors = new Color[numOfPoints];
            Vector3[] normals = new Vector3[numOfPoints];
            float x = 0f;
            float y = 0f;
            float z = 0f;

            float normal_x = 0f;
            float normal_y = 0f;
            float normal_z = 0f;

            float r = 0f;
            float g = 1f;
            float b = 0f;
            float a = 1f;
            bool hasNormals = false;
            using (var fileStream = File.OpenRead(path))
            {
                fileStream.Seek(headerOffset, SeekOrigin.Begin);
                var data = new byte[numOfPoints * fields.Count * 4];
                fileStream.Read(data, 0, data.Length);
                int i = 0;
                using (Stream stream = new MemoryStream(data))
                {
                    while (i < numOfPoints)
                    {
                        foreach ((string field, int size) in fields)
                        {
                            var buffer = new byte[size];
                            stream.Read(buffer, 0, buffer.Length);
                            switch (field.ToLower())
                            {
                                case "x":
                                {
                                    x = BitConverter.ToSingle(buffer, 0);
                                    break;
                                }

                                case "y":
                                {
                                    y = BitConverter.ToSingle(buffer, 0);
                                    break;
                                }

                                case "z":
                                {
                                    z = BitConverter.ToSingle(buffer, 0);
                                    break;
                                }

                                case "normal_x":
                                {
                                    normal_x = BitConverter.ToSingle(buffer, 0);
                                    break;
                                }

                                case "normal_y":
                                {
                                    normal_y = BitConverter.ToSingle(buffer, 0);
                                    break;
                                }

                                case "normal_z":
                                {
                                    hasNormals = true;
                                    normal_z = BitConverter.ToSingle(buffer, 0);
                                    break;
                                }

                                case "rgb":
                                {
                                    r = (float) (buffer[0] / 255.0f);
                                    g = (float) (buffer[1] / 255.0f);
                                    b = (float) (buffer[2] / 255.0f);
                                    break;
                                }

                                case "rgba":
                                {
                                    r = (float) (buffer[0] / 255.0f);
                                    g = (float) (buffer[1] / 255.0f);
                                    b = (float) (buffer[2] / 255.0f);
                                    a = (float) (buffer[3]);
                                    break;
                                }
                            }
                        }
                        points[i] = new Vector3(x, z, y);
                        normals[i] = new Vector3(normal_x, normal_z, normal_y);
                        colors[i] = new Color(b, g, r, a);
                        i++;
                    }
                }
            }
            PointCloud p = new PointCloud(points);
            if (hasNormals)
            {
                p.Normals = normals;
            }
            p.Colors = colors;
            return p;
        }

        private static PointCloud ReadBinaryCompressedPoints(string path, int headerOffset, int numOfPoints, List<(string, int)> fields)
        {
            Vector3[] points = new Vector3[numOfPoints];
            Color[] colors = new Color[numOfPoints];
            Vector3[] normals = new Vector3[numOfPoints];
            bool hasNormals = false;
            using (var fileStream = File.OpenRead(path))
            {
                fileStream.Seek(headerOffset, SeekOrigin.Begin);
                var tmp = new byte[4];

                fileStream.Read(tmp, 0, tmp.Length);
                uint compressedSize = BitConverter.ToUInt32(tmp, 0);

                fileStream.Read(tmp, 0, tmp.Length);
                uint uncompressedSize = BitConverter.ToUInt32(tmp, 0);

                var data = new byte[compressedSize];
                fileStream.Read(data, 0, data.Length);
                byte[] udata = CLZF2.Decompress(data);

                var floatArr = new float[udata.Length / 4];
                Buffer.BlockCopy(udata, 0, floatArr, 0, udata.Length);

                float x = 0f;
                float y = 0f;
                float z = 0f;
                float normal_x = 0f;
                float normal_y = 0f;
                float normal_z = 0f;
                float r = 0f;
                float g = 1f;
                float b = 0f;
                float a = 1f;
                for (var i = 0; i < numOfPoints; i++)
                {
                    a = 1f;
                    for (int j = 0; j < fields.Count;  j++)
                    {
                        (var field, int size) = fields[j];
                        if (field.ToLower() == "x")
                        {
                            x = floatArr[i + j*numOfPoints];
                            continue;
                        }
                        else if (field.ToLower() == "y")
                        {
                            y = floatArr[i + j*numOfPoints];
                            continue;
                        }
                        else if (field.ToLower() == "z")
                        {
                            z = floatArr[i + j*numOfPoints];
                            continue;
                        }
                        else if (field.ToLower() == "normal_x")
                        {
                            normal_x = floatArr[i + j*numOfPoints];
                            continue;
                        }
                        else if (field.ToLower() == "normal_y")
                        {
                            normal_y = floatArr[i + j*numOfPoints];
                            continue;
                        }
                        else if (field.ToLower() == "normal_z")
                        {
                            hasNormals = true;
                            normal_z = floatArr[i + j*numOfPoints];
                            continue;
                        }
                        else if (field.ToLower() == "rgb")
                        {
                            var rgb = floatArr[i + j*numOfPoints];
                            var bytes = BitConverter.GetBytes(rgb);
                            r = (float) (bytes[0] / 255.0f);
                            g = (float) (bytes[1] / 255.0f);
                            b = (float) (bytes[2] / 255.0f);
                            continue;
                        }
                        else if (field.ToLower() == "rgba")
                        {
                            var rgba = floatArr[i + j*numOfPoints];
                            var bytes = BitConverter.GetBytes(rgba);
                            r = (float) (bytes[0] / 255.0f);
                            g = (float) (bytes[1] / 255.0f);
                            b = (float) (bytes[2] / 255.0f);
                            a = (float) (bytes[3]);
                            continue;
                        }
                    }
                    points[i] = new Vector3(x, z, y);
                    normals[i] = new Vector3(normal_x, normal_z, normal_y);
                    colors[i] = new Color(b, g, r, a);
                }
            }
            PointCloud p = new PointCloud(points);
            if (hasNormals)
            {
                p.Normals = normals;
            }
            p.Colors = colors;
            return p;
        }

        private static (int, string, List<(string, int)>, int) ReadMetadataHeader(string path)
        {
            List<(string, int)> fields = new List<(string, int)>();
            string format = "";
            int numOfPoints = 0;
            const Int32 BufferSize = 128;
            int length = 0;
            using (var fileStream = File.OpenRead(path))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.ASCII, false, BufferSize)) 
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        length += line.Length + 1;
                        if (line.StartsWith("#")) continue;
                        if (line.ToLower().StartsWith("fields"))
                        {
                            List<string> flds = line.Split(' ').ToList();
                            flds.RemoveAt(0);
                            foreach (var f in flds)
                            {
                                fields.Add((f, 0));
                            }
                        }
                        if (line.ToLower().StartsWith("size"))
                        {
                            var sizes = line.Split(' ').ToList();
                            sizes.RemoveAt(0);
                            for (int i = 0; i < fields.Count; i++)
                            {
                                fields[i] = (fields[i].Item1, Int32.Parse(sizes[i]));
                            }

                        }
                        if (line.ToLower().StartsWith("points")){
                            numOfPoints = Int32.Parse(line.Split(' ')[1]);
                            continue;
                        }
                        if (line.ToLower().StartsWith("data"))
                        {
                            format = line.Split(' ')[1];
                            break;
                        }
                    }
                }
            }
            if (numOfPoints == 0 || format == "")
            {
                throw new Exception($"Could not parse metadata header: {path}");
            }
            return (numOfPoints, format, fields, length);
        }

        private static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            return new ArraySegment<T>(array, offset, length)
                        .ToArray();
        }
    }

    /// <summary>
    /// Pointcloud object
    /// </summary>
    public class PointCloud
    {
        /// <summary>
        /// Holds point data (x, y, z)
        /// </summary>
        public Vector3[] Points;
        #nullable enable
        /// <summary>
        /// Holds normals data (x, y, z)
        /// </summary>
        public Vector3[]? Normals = null;

        /// <summary>
        /// Holds color data
        /// </summary>
        public Color[]? Colors = null;
        #nullable disable

        /// <summary>
        /// Number of points
        /// </summary>
        public int Length;

        public PointCloud()
        {

        }

        public PointCloud(Vector3[] points)
        {
            this.Points = points;
        }


        public PointCloud(Vector3[] points, Color[] colors) : this(points)
        {
            this.Colors = colors;
        }

        public PointCloud(Vector3[] points, Color[] colors, Vector3[] normals) : this(points, colors)
        {
            this.Normals = normals;
        }

        public PointCloud(Vector3[] points, Vector3[] normals) : this(points)
        {
            this.Normals = normals;
        }
    }
}
