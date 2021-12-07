using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Labust.UI
{
    /// <summary>
    /// UI script that controls the UI depth information
    /// </summary>
    public class Depth : MonoBehaviour
    {

        public GameObject Player;

        Text _depthText;
        string depthPrefix;

        // Start is called before the first frame update
        void Start()
        {
            _depthText = GetComponent<Text>();
            depthPrefix = _depthText.text;
        }

        // Update is called once per frame
        void Update()
        {
            _depthText.text = $"{depthPrefix} {Mathf.Max(-Player.transform.position.y, 0f):0.##}";
        }
    }
}