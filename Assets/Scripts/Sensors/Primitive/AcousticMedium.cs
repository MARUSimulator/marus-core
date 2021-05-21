using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Simulator.Sensors
{
    public class AcousticMedium : MonoBehaviour
    {
        public List<Nanomodem> nanomodems = new List<Nanomodem>();
        public Dictionary<int, Nanomodem> modems = new Dictionary<int, Nanomodem>();

        protected void Awake()
        {
            foreach (var nanomodem in nanomodems) 
            {
                Debug.Log("Adding nanomodem id:" + nanomodem.id + " to dictionary");
                modems.Add(nanomodem.id, nanomodem);
            }
        }
    }
}