using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Grpc.Core;
using Labust.Networking;
using Remotecontrol;
using UnityEngine;

namespace Labust.Actuators
{
    public class ControllerBase : MonoBehaviour
    {

        protected Rigidbody body;

        /// <summary>
        /// Vehichle that sensor is attached to
        /// Gets rigid body component of a first ancestor
        /// </summary>
        /// <value></value>
        public Transform vehicle
        {
            get
            {
                if (body == null)
                {
                    var component = GetComponent<Rigidbody>();
                    if (component != null)
                        body = component;
                    else
                        body = Utils.Helpers.GetParentRigidBody(transform);
                }
                return body.transform;
            }
        }
    }
}