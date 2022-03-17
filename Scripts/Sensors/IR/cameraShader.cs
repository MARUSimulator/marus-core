// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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