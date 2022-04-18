using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Marus.Communications.Rf
{
    public class LoraRangeing : MonoBehaviour
    {


        public LoraDevice Master;
        List<LoraDevice> _rangeingDevices;
        public List<LoraDevice> Targets;

        // Start is called before the first frame update
        void Awake()
        {
            if (Master == null)
            {
                Debug.Log("Rangeing error. The  master for rangeing is not set.");
                this.enabled = false;
                return;
            }
            // _rangeingDevices = Get
            _rangeingDevices = GetComponentsInChildren<LoraDevice>(false).ToList();
            _rangeingDevices.RemoveAll(x => x.GetInstanceID() == Master.GetInstanceID());
        }

        // Update is called once per frame
        void Update()
        {
            foreach(var target in Targets)
            {
                foreach (var d in _rangeingDevices)
                {
                    var r = RfMediumHelper.Range(d, target);
                }
            }
        }
    }
}

