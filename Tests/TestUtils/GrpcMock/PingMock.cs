
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Parameterserver;
using Ping;
using Std;

namespace TestUtils
{

    public class PingMock : Ping.Ping.PingBase
    {

        public override Task<PingMsg> Ping(PingMsg request, ServerCallContext context)
        {
            return Task.FromResult(new PingMsg{ Value = 1 });
        }
    }
}
