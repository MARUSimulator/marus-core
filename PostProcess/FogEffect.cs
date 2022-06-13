using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
// #if CREST_OCEAN
//     using Crest;
// #endif
using Marus.Ocean;

[Serializable, VolumeComponentMenu("Post-processing/Custom/FogEffect")]
public sealed class FogEffect : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ColorParameter fogColor = new ColorParameter(Color.blue);

    Material m_Material;

    float waterLevelHeight = 0f;

    public bool IsActive()
    {
        waterLevelHeight = WaterHeightSampler.Instance.GetWaterLevel(Camera.current.transform.position);
        if (m_Material != null && waterLevelHeight < 0f)
        {
            return true;
        }
        return false;
    }

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/FogEffect";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume FogEffect is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_Intensity", intensity.value);
        m_Material.SetColor("_FogColorr", fogColor.value);

        cmd.Blit(source, destination, m_Material, 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
