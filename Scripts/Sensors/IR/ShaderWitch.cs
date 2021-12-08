using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderWitch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown("space"))
        {
            if (Shader.IsKeywordEnabled("HEATMAP_ON"))
            {
                Shader.DisableKeyword("HEATMAP_ON");
                Debug.Log("heatmap off");
            }
            else
            {
                //Shader.DisableKeyword("HEATMAP_ON");
                Shader.EnableKeyword("HEATMAP_ON");
                Debug.Log("heatmap on");
            }
            Debug.Log("inne");
            
        }
    }
}
