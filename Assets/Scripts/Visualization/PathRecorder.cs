using System;
using UnityEngine;
using System.IO;
using Labust.Utils;
using Labust.Logger;

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

        private bool _enabled = true;

        ///	<summary>
        /// Minimum distance in meters between points to be recorded.
        /// </summary>
        private float MinimumDistanceDelta = 0.1f;
        private float _timer = 0f;
        private string _fileName;
        private Vector3 _lastPosition;
        private GameObjectLogger<Vector3> logger;
        private string topic;

        void Start()
        {
            topic = $"PathRecording-{gameObject.name}";
            logger = DataLogger.Instance.GetLogger<Vector3>(topic);
        }

        void Update()
        {
            if (!_enabled)
            {
                return;
            }

            var distanceDeltaCondition = Vector3.Distance(_lastPosition, transform.position) > MinimumDistanceDelta;
            if (_timer >= (1 / SampleRateHz) && distanceDeltaCondition)
            {
                logger.Log(transform.position);
                _lastPosition = transform.position;
                _timer = 0;
            }
            _timer += Time.deltaTime;
        }

        public bool IsEnabled()
        {
            return _enabled;
        }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
            OnDisable();
        }

        void OnDisable()
        {
            string savePath = Path.Combine(Application.dataPath, "PathRecordings");
            DataLoggerUtilities.SaveLogsForTopic(topic, savePath);
        }
    }
}
