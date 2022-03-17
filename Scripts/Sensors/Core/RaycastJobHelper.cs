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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Labust.Sensors
{

    public abstract class RaycastJobHelper
    {

        /// <summary>
        /// Set evenly distributed array of ray directions for configured field of view and resolution 
        /// </summary>
        public static NativeArray<Vector3> EvenlyDistributeRays(int widthRes, int heightRes, float horizontalFieldOfView, float verticalFieldOfView)
        {
            NativeArray<Vector3> directionsLocal = new NativeArray<Vector3>(widthRes * heightRes, Allocator.Persistent);
            var hfov = horizontalFieldOfView * Mathf.Deg2Rad;
            var vfov = verticalFieldOfView * Mathf.Deg2Rad;
            var hfovOverTwo = hfov / 2;
            var vfovOverTwo = vfov / 2;
            for (var i = 0; i < widthRes; i++)
            {
                var horizontalAngle = (widthRes == 1) ? 0
                    : i * hfov / (widthRes - 1) - hfovOverTwo;
                for (var j = 0; j < heightRes; j++)
                {
                    var verticalAngle = (heightRes == 1) ? 0  
                        : j * vfov / (heightRes - 1) - vfovOverTwo;

                    var sinhor = Mathf.Sin(horizontalAngle);
                    var coshor = Mathf.Cos(horizontalAngle);
                    var sinver = Mathf.Sin(verticalAngle);
                    var cosver = Mathf.Cos(verticalAngle);


                    directionsLocal[i * heightRes + j] = new Vector3(cosver * sinhor, sinver, cosver * coshor); // y is up; y angle is (90 - theta) in spherical 
                }
            }
             
            return directionsLocal;
        }
    }


    public class RaycastJobHelper<T> : RaycastJobHelper where T: struct
    {
        NativeArray<Vector3> _directionsLocal;
        NativeArray<Vector3> _directionsGlobal;
        NativeArray<RaycastCommand> _commands;
        NativeArray<RaycastHit> _hits;
        NativeArray<Vector3> _points;
        NativeArray<T> _results;

        JobHandle _raycastHandle;
        JobHandle _readbackHandle;
        bool _readbackInProgress;
        bool _hasData;
        GameObject _obj;
        private float _maxDistance;
        
        Action<NativeArray<Vector3>, NativeArray<T>> _onFinishCallback;
        static Dictionary<int, Func<RaycastHit, Vector3, int, T>> _getResultFromHit;
        

        public RaycastJobHelper(GameObject obj, NativeArray<Vector3> directions, 
                Func<RaycastHit, Vector3, int, T> getResultFromHit,
                Action<NativeArray<Vector3>, NativeArray<T>> onFinish,
                float maxDistance=float.MaxValue)
                
        {
            var totalRays = directions.Length;
            _obj = obj;
            _directionsLocal = directions;
            _directionsGlobal = new NativeArray<Vector3>(totalRays, Allocator.Persistent);
            _hasData = false;
            _commands = new NativeArray<RaycastCommand>(totalRays, Allocator.Persistent);
            _hits = new NativeArray<RaycastHit>(totalRays, Allocator.Persistent);
            _results = new NativeArray<T>(totalRays, Allocator.Persistent);
            _points = new NativeArray<Vector3>(totalRays, Allocator.Persistent);
            _maxDistance = maxDistance;
            InitializeGetResultFromHit(getResultFromHit);
            _onFinishCallback = onFinish;

        }

        private void InitializeGetResultFromHit(Func<RaycastHit, Vector3, int, T> getResultFromHit)
        {
            if (_getResultFromHit == null)
            {
                _getResultFromHit = new Dictionary<int, Func<RaycastHit, Vector3, int, T>>();
            }
            _getResultFromHit.Add(_obj.GetInstanceID(), getResultFromHit);
        }

        public void RaycastSync()
        {
            if (_raycastHandle.IsCompleted && !_readbackInProgress)
            {
                _raycastHandle = ScheduleNewRaycastJob();
                _readbackHandle = ReadbackData();
                _readbackInProgress = true;
            }
            _readbackHandle.Complete();
            if (_readbackHandle.IsCompleted)
            {
                _readbackHandle.Complete();
                _readbackInProgress = false;
                _onFinishCallback(_points, _results);
                _hasData = true;
            }
        }

        public IEnumerator RaycastInLoop()
        {
            while (true)
            {
                if (_raycastHandle.IsCompleted && !_readbackInProgress)
                {
                    _raycastHandle = ScheduleNewRaycastJob();
                    _readbackHandle = ReadbackData();
                    _readbackInProgress = true;
                }
                yield return new WaitForEndOfFrame();

                if (_readbackHandle.IsCompleted)
                {
                    _readbackHandle.Complete();
                    
                    _readbackInProgress = false;
                    _onFinishCallback(_points, _results);
                    _hasData = true;
                }
                yield return null;
            }
        }

        public void Dispose()
        {
            _raycastHandle.Complete();
            _readbackHandle.Complete();

            // dispse allocated buffers
            _commands.Dispose();
            _hits.Dispose();
            _directionsLocal.Dispose();
            _directionsGlobal.Dispose();
            _results.Dispose();
            _points.Dispose();
        }

        private JobHandle ReadbackData()
        {
            var readback = new ReadbackDataJob();
            readback.hits = _hits;
            readback.results = _results;
            readback.points = _points;
            readback.directions = _directionsLocal;
            readback.objectId = _obj.GetInstanceID();
            return readback.Schedule(_hits.Length, 10, _raycastHandle);
        }

        private JobHandle ScheduleNewRaycastJob()
        {
            var transform = _obj.transform;
            // var inverseRotation = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).inverse;

            for (int i=0; i<_directionsLocal.Length; i++)
            {
                _directionsGlobal[i] = transform.TransformDirection(_directionsLocal[i]);
            }
            var commandsJob = new CreateRaycastCommandsJob();
            commandsJob.commands = _commands;
            commandsJob.maxDistance = _maxDistance;
            commandsJob.directions = _directionsGlobal;
            commandsJob.position = transform.position;
            var commandsJobHandle = commandsJob.Schedule(_directionsLocal.Length, 10);


            return RaycastCommand.ScheduleBatch(_commands, _hits, 10, commandsJobHandle);
        }

        // Job cannot have reference type fields, so it calles one global static method to get result
        // This method then decides what Func to call
        public static T GetResultFromHit(int objId, RaycastHit hit, Vector3 direction, int index)
        {
            return _getResultFromHit[objId](hit, direction, index);
        }

        public struct CreateRaycastCommandsJob : IJobParallelFor
        {
            [ReadOnly]
            public Vector3 position;

            [ReadOnly]
            public NativeArray<Vector3> directions;

            [ReadOnly]
            public float maxDistance;

            public NativeArray<RaycastCommand> commands;

            public void Execute(int i)
            {
                commands[i] = new RaycastCommand(position, directions[i], maxDistance);
            }
        }

        public struct ReadbackDataJob : IJobParallelFor
        {

            [ReadOnly]
            public NativeArray<RaycastHit> hits;
            public NativeArray<Vector3> directions;
            public NativeArray<T> results;
            public NativeArray<Vector3> points;
            public int objectId;

            public void Execute(int i)
            {
                points[i] = hits[i].point;
                results[i] = GetResultFromHit(objectId, hits[i], directions[i], i);
            }
        }
    }
}
