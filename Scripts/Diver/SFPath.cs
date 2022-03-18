using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFPath : MonoBehaviour
{
    public LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.SetColors(c1, c2);
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        // lineRenderer.SetWidth(0.2F, 0.2F);
        int i = 0;
        var theta_scale = 0.1f;
        lineRenderer.positionCount = Convert.ToInt32(Math.Ceiling(2*Math.PI/theta_scale + 1));
        var r = 3.0f;
        for(float theta = 0; theta <= 2 * Math.PI + theta_scale; theta += theta_scale) {
            var x = (float)(r*Math.Cos(theta));
            var y = (float)(r*Math.Sin(theta));

            Vector3 pos = new Vector3(x, 0 , y);
            lineRenderer.SetPosition(i, pos);
            i+=1;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
