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
      public TestClient(grpc::ChannelBase channel) : base(channel)
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
