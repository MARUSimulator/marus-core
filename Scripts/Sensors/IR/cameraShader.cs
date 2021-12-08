using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraShader : MonoBehaviour
{
    public bool heatmapOn = false;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
        //RenderPipelineManager.beginFrameRendering += BeginFrameRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += EndCameraRendering;
        //RenderPipelineManager.beginFrameRendering -= BeginFrameRendering;
    }

    void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (heatmapOn)
        {
            Shader.EnableKeyword("HEATMAP_ON");
            Debug.Log("HEATMAP_ON");
        }
        else
        {
            Shader.DisableKeyword("HEATMAP_ON");
            Debug.Log("HEATMAP_OFF");
        }
    }

    void EndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (heatmapOn)
        {
            Shader.DisableKeyword("HEATMAP_ON");
        }
        else
        {
            Shader.EnableKeyword("HEATMAP_ON");
        }
    }

}