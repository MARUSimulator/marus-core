using System;
using UnityEngine;
using System.IO;
using Labust.Utils;

namespace Labust.Visualization
{
	public class PathRecorder : MonoBehaviour
	{
		public float SampleRateHz = 5;
		private float MinimumTranslationDistance = 0.1f;
		private float _timer = 0f;
		private string _fileName;
		private Vector3 _lastPosition;

		void Start()
		{
			string timeRepr = DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");
			string PathRecordingsPath = Path.Combine(Application.dataPath, "PathRecordings");
			_fileName = PathRecordingsPath + "/PathRecording-" + timeRepr + ".bin";
			_lastPosition = transform.position;
			if (!Directory.Exists(PathRecordingsPath))
            {
                Directory.CreateDirectory(PathRecordingsPath);
            }
		}

		void Update()
		{
			var distanceDeltaCondition = Vector3.Distance(_lastPosition, transform.position) < MinimumTranslationDistance;
			if (_timer >= (1 / SampleRateHz) && distanceDeltaCondition)
			{
				using (PathRecordingBinaryWriter writer = new PathRecordingBinaryWriter(File.Open(_fileName, FileMode.Append)))
				{
					TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970,1,1,0,0,0));
					double secs = span.TotalSeconds;
					writer.WriteVector(transform.position, secs);
				}
				_lastPosition = transform.position;
				_timer = 0;
			}
			_timer += Time.deltaTime;
		}
	}
}
