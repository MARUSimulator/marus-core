using System.Threading.Tasks;
using Grpc.Core;
using Std;
using Tf;
using static Tf.Tf;

namespace TestUtils
{

    public class TfMock : TfBase
    {

        TfFrameList _frames;

        public TfMock(TfFrameList frames)
        {
            _frames = frames;
        }

        public override Task<TfFrameList> GetAllFrames(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_frames);
        }

    }
}
