using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Labust.Sensors.Primitive
{
    // TODO
    public class AcousticMedium
    {

        int currentId = 0;
        Dictionary<int, Nanomodem> nanomodems;

        static AcousticMedium _instance;
        public static AcousticMedium Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AcousticMedium();
                return _instance;
            }
        }

        private AcousticMedium()
        {
            nanomodems = new Dictionary<int, Nanomodem>();
        }

        public void RegisterNanomodem(Nanomodem modem)
        {
            nanomodems.Add(currentId++, modem);
        }

        public (bool, float) GetRangeAndValidityToId(Nanomodem fromModem, Nanomodem toModem)
        {
            var range = Vector3.Distance(fromModem.transform.position, toModem.transform.position);

            if (range > fromModem.maxRange)
            {
                Debug.Log($"ID: {fromModem.name} is too far.");
                return (false, range);
            }

            return (true, range);
        }
    }
}