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
using Grpc.Core;
using static Parameterserver.ParameterServer;

using Parameterserver;
using Marus.Utils;

namespace Marus.Networking
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
            ParamValue paramValue;
            try
            {
                paramValue = _parameterServerClient.GetParameter(new GetParamRequest() { Name = name });
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
            }
            catch
            {
                outValue = default(T);
                return false;
            }
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
            try
            {
                _parameterServerClient.SetParameter(request);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
