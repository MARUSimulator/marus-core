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


using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Parameterserver;
using Std;

namespace TestUtils
{

    public class ParameterServerMock : ParameterServer.ParameterServerBase
    {

        Dictionary<string, object> _parameters;

        public ParameterServerMock(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public override Task<ParamValue> GetParameter(GetParamRequest request, ServerCallContext context)
        {
            var par = _parameters[request.Name];
            var response = new ParamValue();
            var typ = par.GetType();
            if (typ == typeof(double))
                response.ValueDouble = (double)par;
            if (typ == typeof(int))
                response.ValueInt = (int)par;
            if (typ == typeof(string))
                response.ValueStr = (string)par;
            if (typ == typeof(bool))
                response.ValueBool = (bool)par;
            return Task.FromResult(response);
        }

        public override Task<Empty> SetParameter(SetParamRequest request, ServerCallContext context)
        {
            _parameters[request.Name] = request.Value.ToString();
            return Task.FromResult(new Empty());
        }
    }
}
