
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
