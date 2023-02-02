// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
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


using System;
using Marus.Networking;
using Marus.Utils;
using UnityEngine;
using Marus.CustomInspector;

namespace Marus.Core
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

        public double TimeDouble => _isRealTime ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
                : _totalTimeSecs + 1e-9 * _totalTimeNsecs;

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
        private void AdaptSimulationSpeed()
        {
            // var a = Time.smoothDeltaTime;
            Time.timeScale = SimulationSpeed;
        }

        private void UpdateTime()
        {
            var deltaTimeSecs = (uint)Time.fixedDeltaTime; // truncate after decimal
            var deltaTimeNsecs = Convert.ToUInt32((Time.fixedDeltaTime - deltaTimeSecs) * SEC2NSEC);

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
