using UnityEngine;
using System;
using System.Collections.Generic;
using Labust.Sensors.Primitive.GenericMedium;
namespace Labust.Sensors.AIS
{
    /// <summary>
    /// This class serves as a radio medium for AIS purposes.
    /// Script should be attached to a GameObject so no singleton is needed.
    /// </summary>
    public class AISManager: Medium<AISMessage>
    {

        protected AISManager() { }
        public string Name = "Radio Medium for AIS";
    }
}
