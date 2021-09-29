using System;
using System.Collections.Generic;
using Grpc.Core;
using UnityEngine;
using System.Threading;
using System.Linq;
using Labust.Core;
using static Tf.Tf;
using static Sensorstreaming.SensorStreaming;
using static Remotecontrol.RemoteControl;
using static Ping.Ping;
using static Parameterserver.ParameterServer;
using static Simulationcontrol.SimulationControl;

using Parameterserver;
using System.Collections;
using Simulationcontrol;
using Labust.Utils;

namespace Labust.Networking
{

    /// <summary>
    /// Singleton class for configuring and connecting to 
    /// ROS server
    /// </summary>
    public class ParamServerHandler : Singleton<ParamServerHandler>
    {
        private ParameterServerClient _parameterServerClient;


        private ParamServerHandler _instance;
        protected override void Initialize()
        {
            transform.parent = RosConnection.Instance.transform;
            RosConnection.Instance.OnConnected += OnConnected;
        }

        public void OnConnected(Channel channel)
        {
            _parameterServerClient = new ParameterServerClient(channel);
        }

        public bool TryGetParameter<T>(string name, out T outValue)
        {
            object value = null;
            outValue = default(T);
            var paramValue = _parameterServerClient.GetParameter(new GetParamRequest() { Name = name });
            switch (paramValue.ParameterValueCase)
            {
                case ParamValue.ParameterValueOneofCase.None:
                    return false;
                case ParamValue.ParameterValueOneofCase.ValueBool:
                    value = paramValue.ValueBool;
                    break;
                case ParamValue.ParameterValueOneofCase.ValueInt:
                    value = paramValue.ValueInt;
                    break;
                case ParamValue.ParameterValueOneofCase.ValueDouble:
                    value = paramValue.ValueDouble;
                    break;
                case ParamValue.ParameterValueOneofCase.ValueStr:
                    value = paramValue.ValueStr;
                    break;
            }
            outValue = (T)value;
            return true;
        }

        public bool TrySetParameter<T>(string name, object value)
        {
            var request = new SetParamRequest
            {
                Name = name,
                Value = new ParamValue()
            };

            switch (value)
            {
                case bool t1:
                    request.Value.ValueBool  = t1;
                    break;
                case int t2:
                    request.Value.ValueInt = t2;
                    break;
                case double t3:
                    request.Value.ValueDouble = t3;
                    break;
                case string t4:
                    request.Value.ValueStr = t4;
                    break;
                default:
                    return false;
            }
            _parameterServerClient.SetParameter(request);
            return true;
        }

    }

}