using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Remotecontrol;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Concurrent;

public class AUVRosController : MonoBehaviour
{

    public float linSpeed = 2f;
    public float rotSpeed = 700f;
    public string vehId = "veh";
    public int connectionTimeout = 5;


    RemoteControl.RemoteControlClient _client;
    Transform _targetTransform;
    AgentManager _agentManager;

    Thread _handleStreamThread;
    ConcurrentQueue<ForceResponse> _responseBuffer;

    // Start is called before the first frame update
    void Awake()
    {
        _targetTransform = transform;
        _agentManager = GameObject.FindObjectOfType<AgentManager>();
        _client = RosConnection.Instance.GetClient<RemoteControl.RemoteControlClient>();

        _responseBuffer = new ConcurrentQueue<ForceResponse>();
        _handleStreamThread = new Thread(HandleServerStream);
        _handleStreamThread.Start();
    }

    async void HandleServerStream()
    {
        while (!RosConnection.Instance.IsConnected)
        {
            Thread.Sleep(1000);
        }
        // invoke rpc call
        var handle = _client.ApplyForce(new ForceRequest() { VehId = vehId }, cancellationToken:RosConnection.Instance.cancellationToken);
        var stream = handle.ResponseStream;

        while (await stream.MoveNext())
        {
            var current = stream.Current;
            _responseBuffer.Enqueue(current);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_responseBuffer.Count > 0 &&
                _responseBuffer.TryDequeue(out var result))
        {
            UpdateMovement(Time.deltaTime, result);
        }
    }

    void UpdateMovement(float dt, ForceResponse result)
    {
        var speed = linSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= 3f;
        }

        _targetTransform.position += speed * _targetTransform.forward * (Input.GetKey(KeyCode.W) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.forward * (Input.GetKey(KeyCode.S) ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.up * (Input.GetKey(KeyCode.E) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.up * (Input.GetKey(KeyCode.Q) ? 1 : 0) * dt;
        _targetTransform.position -= speed * _targetTransform.right * (Input.GetKey(KeyCode.A) ? 1 : 0) * dt;
        _targetTransform.position += speed * _targetTransform.right * (Input.GetKey(KeyCode.D) ? 1 : 0) * dt;

        { // rotation
            float rotate = 0f;
            rotate += (Input.GetKey(KeyCode.X) ? 1 : 0);
            rotate -= (Input.GetKey(KeyCode.Y) ? 1 : 0);
            Vector3 ea = _targetTransform.eulerAngles;
            ea.y += 0.1f * rotSpeed * rotate * dt;
            _targetTransform.eulerAngles = ea;
        }
    }
}
