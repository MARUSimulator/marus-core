using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labust.Networking;
using Labust.Sensors;
using Labust.Sensors.Core;
using Labust.Visualization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Sensorstreaming;

/// <summary>
/// Lidar that cast N rays evenly distributed in configured field of view.
/// Implemented using IJobParallelFor on CPU 
/// Can drop performance
/// </summary>
public class RaycastLidar : SensorBase<LidarStreamingRequest>
{

    /// Instantiates 3 Jobs: 
    /// 1) RaycastCommand creation - create raycast commands <see cref="RaycastCommand"> for lidar FoV
    /// 2) RaycastCommand execution
    /// 3) RaycastHit data interpretation - extract points, distances etc.



    /// <summary>
    /// Material set for point cloud display
    /// </summary>
    public Material ParticleMaterial;
 
    public int WidthRes = 1024;

    public int HeightRes = 16;
    public float MaxDistance = 100;
    public float MinDistance = 0.2f;
    public float FieldOfView = 30;

    const float PIOVERTWO = Mathf.PI / 2;
    const float TWOPI = Mathf.PI * 2;




    // Start is called before the first frame update
    PointCloudManager _pointCloudManager;

    NativeArray<Vector3> _directionsLocal;
    NativeArray<RaycastCommand> _commands;
    NativeArray<RaycastHit> _results;

    NativeArray<float> _distances;
    NativeArray<int> _triangleIndices;
    NativeArray<Vector3> _points;

    JobHandle _raycastHandle;
    JobHandle _readbackHandle;
    bool _readbackInProgress;

    void Start()
    {
        streamHandle = streamingClient.StreamLidarSensor(cancellationToken:RosConnection.Instance.cancellationToken);
        // allocate job buffers
        _commands = new NativeArray<RaycastCommand>(WidthRes * HeightRes, Allocator.Persistent);
        _results = new NativeArray<RaycastHit>(WidthRes * HeightRes, Allocator.Persistent);
        _points = new NativeArray<Vector3>(WidthRes * HeightRes, Allocator.Persistent);
        _triangleIndices = new NativeArray<int>(WidthRes * HeightRes, Allocator.Persistent);
        _distances = new NativeArray<float>(WidthRes * HeightRes, Allocator.Persistent);

        _directionsLocal = new NativeArray<Vector3>(WidthRes * HeightRes, Allocator.Persistent);

        SetupLocalDirections();
        CreatePointCloud();

        StartCoroutine(CalculatePointCloud());
    }

    IEnumerator CalculatePointCloud()
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
                _pointCloudManager.UpdatePointCloud(_points);
                hasData = true;
            }
            yield return null;
        }
    }

    private JobHandle ReadbackData()
    {
        var readback = new ReadbackDataJob();
        readback.hits = _results;
        readback.points = _points;
        readback.triangleIndices = _triangleIndices;
        readback.distances = _distances;
        readback.minDistance = MinDistance;
        return readback.Schedule(_results.Length, 10, _raycastHandle);
    }

    private void CreatePointCloud()
    {
        GameObject pointCloud = new GameObject("PointCloud");
        pointCloud.transform.position = Vector3.zero;
        pointCloud.transform.rotation = Quaternion.identity;
        _pointCloudManager = pointCloud.AddComponent<PointCloudManager>();
        _pointCloudManager.particleMaterial = ParticleMaterial;
        _pointCloudManager.computeParticle = FindComputeShader("PointCloudCS");
        _pointCloudManager.SetupPointCloud(WidthRes * HeightRes);
    }

    private ComputeShader FindComputeShader(string shaderName)
    {
        ComputeShader[] compShaders = (ComputeShader[])Resources.FindObjectsOfTypeAll(typeof(ComputeShader));
        for (int i = 0; i < compShaders.Length; i++)
        {
            if (compShaders[i].name == shaderName)
            {
                return compShaders[i];
            }
        }
        throw new UnityException($"Shader {shaderName} not found in the project");
    }

    /// <summary>
    /// Set evenly distributed array of ray directions for configured field of view and resolution 
    /// </summary>
    private void SetupLocalDirections()
    {
        var fov = FieldOfView * Mathf.Deg2Rad;
        var fovOverTwo = fov / 2;
        for (var i = 0; i < WidthRes; i++)
        {
            var horAngle = i * TWOPI / WidthRes;
            for (var j = 0; j < HeightRes; j++)
            {
                var verticalAngle = j * fov / HeightRes - fovOverTwo;
                var sinhor = Mathf.Sin(horAngle);
                var coshor = Mathf.Cos(horAngle);
                var sinver = Mathf.Sin(verticalAngle);
                var cosver = Mathf.Cos(verticalAngle);
                _directionsLocal[i * HeightRes + j] = new Vector3(cosver*coshor, sinver, cosver*sinhor); // y is up; y angle is (90 - theta) in spherical 
            }
        }
    }

    private JobHandle ScheduleNewRaycastJob()
    {
        var inverseRotation = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).inverse;

        var commandsJob = new CreateRaycastCommandsJob();
        commandsJob.commands = _commands;
        commandsJob.directionsLocal = _directionsLocal;
        commandsJob.position = transform.position;
        commandsJob.inverseRotation = inverseRotation;
        commandsJob.maxDistance = MaxDistance;
        var commandsJobHandle = commandsJob.Schedule(_directionsLocal.Length, 10);

        return RaycastCommand.ScheduleBatch(_commands, _results, 10, commandsJobHandle);
    }

    void OnDestroy()
    {
        // dispse allocated buffers
        _commands.Dispose();
        _directionsLocal.Dispose();
        _results.Dispose();
        _points.Dispose();
        _triangleIndices.Dispose();
        _distances.Dispose();
    }

    public override void SendMessage()
    {
        // TBD
        streamWriter.WriteAsync(new LidarStreamingRequest
        {
            
        });
        hasData = false;
    }

    public struct CreateRaycastCommandsJob : IJobParallelFor
    {
        [ReadOnly]
        public Vector3 position;

        [ReadOnly]
        public Matrix4x4 inverseRotation;

        [ReadOnly]
        public NativeArray<Vector3> directionsLocal;
        
        [ReadOnly]
        public float maxDistance;

        public NativeArray<RaycastCommand> commands;

        public void Execute(int i)
        {
            commands[i] = new RaycastCommand(position, inverseRotation * directionsLocal[i], maxDistance);
        }
    }

    public struct ReadbackDataJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<RaycastHit> hits;
        public float minDistance;
        public NativeArray<Vector3> points;
        public NativeArray<int> triangleIndices;
        public NativeArray<float> distances;

        public void Execute(int i)
        {
            var distance = hits[i].distance;
            if (distance < minDistance)
            {
                triangleIndices[i] = -1;
                points[i] = Vector3.zero;
            }
            else
            {
                triangleIndices[i] = hits[i].triangleIndex;
                points[i] = hits[i].point;
            }
            distances[i] = hits[i].distance;
        }
    }
}
