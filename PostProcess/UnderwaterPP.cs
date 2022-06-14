using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using Marus.Ocean;

[Serializable, VolumeComponentMenu("Post-processing/Custom/UnderwaterPP")]
public sealed class UnderwaterPP : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter noiseScale = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter noiseFrequency = new ClampedFloatParameter(0f, 0f, 10f);
    public ClampedFloatParameter noiseSpeed = new ClampedFloatParameter(0f, 0f, 10f);
    public ClampedFloatParameter pixelOffset = new ClampedFloatParameter(0f, 0f, 1f);

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

    const string kShaderName = "Hidden/Shader/UnderwaterPP";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume UnderwaterPP is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_NoiseScale", noiseScale.value);
        m_Material.SetFloat("_NoiseFrequency", noiseFrequency.value);
        m_Material.SetFloat("_NoiseSpeed", noiseSpeed.value);
        m_Material.SetFloat("_PixelOffset", pixelOffset.value);
        cmd.Blit(source, destination, m_Material, 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
