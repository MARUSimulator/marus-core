using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using grpc = global::Grpc.Core;
namespace TestUtils
{
    public class TestClient : grpc::ClientBase<TestClient>
    {

      /// <summary>Creates a new client for TestClient</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public TestClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for TestClient that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public TestClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected TestClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected TestClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

        protected override TestClient NewInstance(ClientBaseConfiguration configuration)
        {
            return new TestClient();
        }
    }
}
