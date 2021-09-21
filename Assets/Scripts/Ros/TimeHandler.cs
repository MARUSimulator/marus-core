
using System;
using Labust.Networking;
using Labust.Utils;
using UnityEngine;
using static Simulatoncontrol.SimulationControl;

namespace Labust.Core
{
    public class TimeHandler : Singleton<TimeHandler>
    {
        const uint SEC2NSEC = 1000000000; 
        const uint SEC2TICKS = 10000000; 

        uint _startTimeSecs;
        uint _startTimeNsecs;
        uint _totalTimeSecs;
        uint _totalTimeNsecs;

        [HideInInspector]
        public bool _isRealTime;
        
        [ConditionalHideInInspector("_isRealTime", true)]
        public float SimulationSpeed;
        public uint CurrentTime { get; private set;}
        public uint TotalTimeSecs => _totalTimeSecs;
        public uint TotalTimeNsecs => _totalTimeNsecs;
        public uint StartTimeSecs => _startTimeSecs;
        public uint StartTimeNsecs => _startTimeNsecs;

        public double TimeDouble => _totalTimeSecs + 1e-9 * _totalTimeNsecs;

        protected override void Initialize()
        {
            transform.parent = RosConnection.Instance.transform;
            _isRealTime = RosConnection.Instance.RealtimeSimulation; 
            SimulationSpeed = RosConnection.Instance.SimulationSpeed;
            SetSimulationTime();
        }

        void FixedUpdate()
        {
            if (!_isRealTime)
            {
                AdaptSimulationSpeed();
            }
            UpdateTime();
        }

        // TODO: Based on FPS, limit maximal simulation speed
        // idk if it is needed. is the physics simulated well? Needs testing 
        void AdaptSimulationSpeed()
        {
            // var a = Time.smoothDeltaTime;
            Time.timeScale = SimulationSpeed;
        }

        public void UpdateTime()
        {
            var deltaTimeSecs = (uint)Time.deltaTime; // truncate after decimal
            var deltaTimeNsecs = Convert.ToUInt32((Time.deltaTime - deltaTimeSecs) * SEC2NSEC);

            var nSecOverflow = (deltaTimeNsecs + _totalTimeNsecs) / SEC2NSEC;
            _totalTimeSecs += deltaTimeSecs + nSecOverflow;
            _totalTimeNsecs = deltaTimeNsecs + _totalTimeNsecs - SEC2NSEC * nSecOverflow;
            CurrentTime = _totalTimeSecs;
        }

        private void SetSimulationTime()
        {
            var currentDateTime = DateTime.Now.ToUniversalTime();
            var unixStandardDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var ticksSinceUnix = currentDateTime.Ticks - unixStandardDateTime.Ticks;
            _startTimeSecs = (uint)(ticksSinceUnix / (SEC2TICKS));

            _startTimeNsecs = (uint)((ticksSinceUnix - _startTimeSecs * SEC2TICKS) * (SEC2NSEC / SEC2TICKS));
            _totalTimeSecs = _startTimeSecs;
            _totalTimeNsecs = _startTimeNsecs;
        }

    }
}