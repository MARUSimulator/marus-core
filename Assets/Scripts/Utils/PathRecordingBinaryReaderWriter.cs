using System;
using System.IO;
using Unity;
using UnityEngine;

namespace Labust.Utils
{
	public class PathRecordingBinaryWriter : BinaryWriter
	{

		public PathRecordingBinaryWriter(FileStream file) : base(file)
		{
		}
		public void WriteVector(Vector3 vec3, double timestamp)
		{
			Write(vec3.x);
			Write(vec3.y);
			Write(vec3.z);
			Write(timestamp);
		}
	}

	public class PathRecordingBinaryReader : BinaryReader
	{

		public PathRecordingBinaryReader(FileStream file) : base(file)
		{
		}

		public (Vector3, double) ReadVector()
		{
			Vector3 vec = new Vector3();
			vec.x = base.ReadSingle();
			vec.y = base.ReadSingle();
			vec.z = base.ReadSingle();
			double timestamp = base.ReadDouble();

			return (vec, timestamp);
		}
	}
}
