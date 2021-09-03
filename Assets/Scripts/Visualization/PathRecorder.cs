using System;
using UnityEngine;
using System.IO;
using Labust.Utils;

namespace Labust.Visualization
{
    /// <summary>
    /// This script records object position at the given rate.
    /// Positions are stored in a binary file in PathRecordings folder with timestamp in file name.
    /// </summary>
    public class PathRecorder : MonoBehaviour
    {
        ///	<summary>
        /// Position sample rate in Hz.
        /// </summary>
        public float SampleRateHz = 5;

        ///	<summary>
        /// Minimum distance in meters between points to be recorded.
        /// </summary>
        private float MinimumDistanceDelta = 0.1f;
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
            var distanceDeltaCondition = Vector3.Distance(_lastPosition, transform.position) > MinimumDistanceDelta;
            if (_timer >= (1 / SampleRateHz) && distanceDeltaCondition)
            {
                WritePositionToFile(transform.position, _fileName);
                _lastPosition = transform.position;
                _timer = 0;
            }
            _timer += Time.deltaTime;
        }

        /// <summary>
        /// Writes given Vector3 position to a filename using BinaryWriter and adds a timestamp.
        /// </summary>
        /// <param name="position">Vector3 position in space.</param>
        /// <param name="filename">Path to the output file.</param>
        public static void WritePositionToFile(Vector3 position, string filename)
        {
            using (PathRecordingBinaryWriter writer = new PathRecordingBinaryWriter(File.Open(filename, FileMode.Append)))
                {
                    TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970,1,1,0,0,0));
                    double secs = span.TotalSeconds;
                    writer.WriteVector(position, secs);
                }
        }
    }
}
