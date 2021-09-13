using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Labust.UI
{
    /// <summary>
    /// UI script that controls the UI compass
    /// </summary>
    public class Compass : MonoBehaviour
    {

        public GameObject Player;

        Vector3 _northDirection;
        Transform _needle;

        // Start is called before the first frame update
        void Awake()
        {
            _needle = transform.GetChild(0);
        }

        // Update is called once per frame
        void Update()
        {
            if (Player != null)
            {
                _needle.rotation = Quaternion.Euler(0, 0, Player.transform.eulerAngles.y);
            }
        }
    }
}