using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomOceanCurrent : MonoBehaviour
{
    // Start is called before the first frame update
    public float MaxCurrent;
    public float MinCurrent;
    void Start()
    {
        GetComponent<Renderer>().material.SetFloat("_Speed", 
            MinCurrent + Random.value*(MaxCurrent-MinCurrent));
        GetComponent<Renderer>().material.SetFloat("_Direction", Random.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
