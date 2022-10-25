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
using System.Collections.Generic;
using Marus.Utils;
using UnityEngine;

namespace Marus.Sensors
{
    [DefaultExecutionOrder(100)]
    public class SensorSampler : Singleton<SensorSampler>
    {

        Dictionary<int, SensorCallback> _sensorCallbacks = new Dictionary<int, SensorCallback>();
        Dictionary<int, double> _timeOfLastCallback = new Dictionary<int, double>();

        void FixedUpdate()
        {
            foreach (var kvp in _sensorCallbacks)
            {
                var callback = kvp.Value;

                if (callback.active
                    && callback.sensor.isActiveAndEnabled)
                {
                    if (!_timeOfLastCallback.TryGetValue(kvp.Key, out var time)
                        || EnoughTimePassed(time, callback.sensor))
                    {
                        callback.callback();
                        callback.sensor.hasData = true;
                        _timeOfLastCallback[kvp.Key] = Time.fixedTimeAsDouble;
                    }
                }
            }
        }

        private bool EnoughTimePassed(double lastTime, SensorBase sensor)
        {
            if (sensor.SampleFrequency <= 0)
                return true;
            return Time.fixedTimeAsDouble >= lastTime + 1 / sensor.SampleFrequency;
        }

        public void AddSensorCallback(SensorBase sensor, Action callback)
        {
            if (_sensorCallbacks.ContainsKey(sensor.GetInstanceID()))
                return;

            var sensorCallback = new SensorCallback
            {
                callback = callback,
                sensor = sensor,
                active = true
            };
            _sensorCallbacks.Add(sensor.GetInstanceID(), sensorCallback);
        }

        private void RemoveSensorCallback(SensorBase sensor)
        {
            _sensorCallbacks.Remove(sensor.GetInstanceID());
        }

        internal void DisableCallback(SensorBase sensor)
        {
            if (_sensorCallbacks.TryGetValue(sensor.GetInstanceID(), out var callback))
            {
                callback.active = false;
            }
        }

        internal void EnableCallback(SensorBase sensor)
        {
            if (_sensorCallbacks.TryGetValue(sensor.GetInstanceID(), out var callback))
            {
                callback.active = true;
            }
        }

    }

    /// <summary>
    /// Data class for sensor callback definition
    /// </summary>
    public class SensorCallback
    {
        public SensorBase sensor;
        // callback for sensor update
        public Action callback;
        public bool active = true;
    };
}