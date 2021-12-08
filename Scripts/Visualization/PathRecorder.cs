using UnityEngine;
using System.IO;
using Labust.Logger;
using Labust.Mission;
using UnityEngine.SceneManagement;

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
        private string savePath;
        private MissionControl missionControl;

        void Start()
        {
            missionControl = GameObject.Find("Mission").GetComponent<MissionControl>();
            savePath = Path.Combine(Application.dataPath, "PathRecordings");
            Scene scene = SceneManager.GetActiveScene();
            RefreshTopic();
            logger = DataLogger.Instance.GetLogger<Vector3>(topic);
        }

        private void RefreshTopic()
        {
            int index;
            string prefix;
            (prefix, index) = GetPathNumber();
            topic = $"{prefix}-{index}-";
        }

        private (string, int) GetPathNumber()
        {
            string [] fileEntries = System.IO.Directory.GetFiles(savePath);
            Scene scene = SceneManager.GetActiveScene();
            string prefix;
            if (scene.name == "BTS")
            {
                prefix = "Classic";
            }
            else if (scene.name == "BTS_Dinis")
            {
                prefix = "Guided";
            }
            else
            {
                prefix = "PathRecording";
            }
            int index = 0;
            foreach(string fileName in fileEntries)
            {
                var f = System.IO.Path.GetFileName(fileName);
                if (f.EndsWith(".json") && f.StartsWith(prefix))
                {
                    Debug.Log(f);
                    index++;
                }
            }

            index++;

            return (prefix, index);
        }

        void Update()
        {
            if (!_enabled)
            {
                return;
            }
            if (missionControl.MissionComplete)
            {
                Disable();
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
            RefreshTopic();
            _enabled = false;
            OnDisable();
        }

        void OnDisable()
        {
            DataLoggerUtilities.SaveLogsForTopic(topic, savePath);
        }
    }
}
