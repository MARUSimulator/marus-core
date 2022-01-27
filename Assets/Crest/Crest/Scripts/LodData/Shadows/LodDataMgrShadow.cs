// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// BIRP fallback not really tested yet - shaders need fixing up.

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if CREST_URP
using UnityEngine.Rendering.Universal;
#endif
#if ENABLE_VR && ENABLE_VR_MODULE
using UnityEngine.XR;
#endif

namespace Crest
{
    using SettingsType = SimSettingsShadow;

    /// <summary>
    /// Stores shadowing data to use during ocean shading. Shadowing is persistent and supports sampling across
    /// many frames and jittered sampling for (very) soft shadows.
    /// </summary>
    public class LodDataMgrShadow : LodDataMgr
    {
        public override string SimName { get { return "Shadow"; } }
        protected override GraphicsFormat RequestedTextureFormat => GraphicsFormat.R8G8_UNorm;
        protected override bool NeedToReadWriteTextureData { get { return true; } }
        static Texture2DArray s_nullTexture => TextureArrayHelpers.BlackTextureArray;
        protected override Texture2DArray NullTexture => s_nullTexture;

        internal const string MATERIAL_KEYWORD_PROPERTY = "_Shadows";
        internal static readonly string MATERIAL_KEYWORD = MATERIAL_KEYWORD_PREFIX + "_SHADOWS_ON";
        internal const string ERROR_MATERIAL_KEYWORD_MISSING = "Shadowing is not enabled on the ocean material and will not be visible.";
        internal const string ERROR_MATERIAL_KEYWORD_MISSING_FIX = "Tick the <i>Shadowing</i> option in the <i>Scattering<i> parameter section on the material currently assigned to the <i>OceanRenderer</i> component.";
        internal const string ERROR_MATERIAL_KEYWORD_ON_FEATURE_OFF = "The shadow feature is disabled on this component but is enabled on the ocean material.";
        internal const string ERROR_MATERIAL_KEYWORD_ON_FEATURE_OFF_FIX = "If this is not intentional, either enable the <i>Create Shadow Data</i> option on this component to turn it on, or disable the <i>Shadowing</i> feature on the ocean material to save performance.";

        public static bool s_processData = true;

        Light _mainLight;

        // SRP version needs access to this externally, hence public get
        public CommandBuffer BufCopyShadowMap { get; private set; }

        RenderTexture _sources;
        PropertyWrapperMaterial[] _renderMaterial;

        readonly int sp_CenterPos = Shader.PropertyToID("_CenterPos");
        readonly int sp_Scale = Shader.PropertyToID("_Scale");
        readonly int sp_JitterDiameters_CurrentFrameWeights = Shader.PropertyToID("_JitterDiameters_CurrentFrameWeights");
        readonly int sp_MainCameraProjectionMatrix = Shader.PropertyToID("_MainCameraProjectionMatrix");
        readonly int sp_SimDeltaTime = Shader.PropertyToID("_SimDeltaTime");
        readonly int sp_LD_SliceIndex_Source = Shader.PropertyToID("_LD_SliceIndex_Source");
        readonly int sp_cascadeDataSrc = Shader.PropertyToID("_CascadeDataSrc");

        public override SimSettingsBase SettingsBase => Settings;
        public SettingsType Settings => _ocean._simSettingsShadow != null ? _ocean._simSettingsShadow : GetDefaultSettings<SettingsType>();

        public LodDataMgrShadow(OceanRenderer ocean) : base(ocean)
        {
            Start();
        }

        public override void Start()
        {
            base.Start();

            {
                _renderMaterial = new PropertyWrapperMaterial[OceanRenderer.Instance.CurrentLodCount];
                var shader = Shader.Find("Hidden/Crest/Simulation/Update Shadow");
                for (int i = 0; i < _renderMaterial.Length; i++)
                {
                    _renderMaterial[i] = new PropertyWrapperMaterial(shader);
                }
            }

#if UNITY_EDITOR
            if (OceanRenderer.Instance.OceanMaterial != null
                && OceanRenderer.Instance.OceanMaterial.HasProperty(MATERIAL_KEYWORD_PROPERTY)
                && !OceanRenderer.Instance.OceanMaterial.IsKeywordEnabled(MATERIAL_KEYWORD))
            {
                Debug.LogWarning("Crest: " + ERROR_MATERIAL_KEYWORD_MISSING + " " + ERROR_MATERIAL_KEYWORD_MISSING_FIX, _ocean);
            }
#endif

#if CREST_URP
            var asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (asset && asset.shadowCascadeCount < 2)
            {
                Debug.LogError("Crest shadowing requires shadow cascades to be enabled on the pipeline asset. "
                    + $"See documentation here: {Internal.Constants.HELP_URL_BASE_USER}ocean-simulation.html#shadows", asset);
                enabled = false;
                return;
            }
#endif

            // Enable sample shadows custom pass.
            if (RenderPipelineHelper.IsHighDefinition)
            {
#if CREST_HDRP
                SampleShadowsHDRP.Enable();
#endif
            }
            else if (RenderPipelineHelper.IsUniversal)
            {
#if CREST_URP
                SampleShadowsURP.Enable();
#endif
            }
        }

        protected override void InitData()
        {
            base.InitData();

            int resolution = OceanRenderer.Instance.LodDataResolution;
            var desc = new RenderTextureDescriptor(resolution, resolution, CompatibleTextureFormat, 0);
            _sources = CreateLodDataTextures(desc, SimName + "_1", NeedToReadWriteTextureData);

            TextureArrayHelpers.ClearToBlack(_sources);
            TextureArrayHelpers.ClearToBlack(_targets);
        }

        bool StartInitLight()
        {
            if (_mainLight == null)
            {
                _mainLight = OceanRenderer.Instance._primaryLight;

                if (_mainLight == null)
                {
                    if (!Settings._allowNullLight)
                    {
                        Debug.LogWarning("Crest: Primary light must be specified on OceanRenderer script to enable shadows.", OceanRenderer.Instance);
                    }
                    return false;
                }

                if (_mainLight.type != LightType.Directional)
                {
                    Debug.LogError("Crest: Primary light must be of type Directional.", OceanRenderer.Instance);
                    return false;
                }

                if (_mainLight.shadows == LightShadows.None)
                {
                    Debug.LogError("Crest: Shadows must be enabled on primary light to enable ocean shadowing (types Hard and Soft are equivalent for the ocean system).", OceanRenderer.Instance);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// May happen if scenes change etc
        /// </summary>
        void ClearBufferIfLightChanged()
        {
            if (_mainLight != OceanRenderer.Instance._primaryLight)
            {
                if (_mainLight)
                {
                    BufCopyShadowMap = null;
                    TextureArrayHelpers.ClearToBlack(_sources);
                    TextureArrayHelpers.ClearToBlack(_targets);
                }
                _mainLight = null;
            }
        }

        public override void UpdateLodData()
        {
            if (!enabled)
            {
                return;
            }

            base.UpdateLodData();

            ClearBufferIfLightChanged();

            if (!StartInitLight())
            {
                enabled = false;
                return;
            }

            if (!s_processData)
            {
                return;
            }

            if (BufCopyShadowMap == null)
            {
                BufCopyShadowMap = new CommandBuffer();
                BufCopyShadowMap.name = "Shadow data";
            }

            if (!s_processData)
            {
                return;
            }

            Swap(ref _sources, ref _targets);

            BufCopyShadowMap.Clear();

            ValidateSourceData();

            // clear the shadow collection. it will be overwritten with shadow values IF the shadows render,
            // which only happens if there are (nontransparent) shadow receivers around. this is only reliable
            // in play mode, so don't do it in edit mode.
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
#endif
            {
                TextureArrayHelpers.ClearToBlack(_targets);
            }

            // Cache the camera for further down.
            var camera = OceanRenderer.Instance.ViewCamera;
            if (camera == null)
            {
                // We want to return early after clear.
                return;
            }

#if CREST_SRP
#pragma warning disable 618
            using (new ProfilingSample(BufCopyShadowMap, "CrestSampleShadows"))
#pragma warning restore 618
#endif
            {
                var lt = OceanRenderer.Instance._lodTransform;
                for (var lodIdx = lt.LodCount - 1; lodIdx >= 0; lodIdx--)
                {
#if UNITY_EDITOR
                    lt._renderData[lodIdx].Validate(0, SimName);
#endif

                    _renderMaterial[lodIdx].SetVector(sp_CenterPos, lt._renderData[lodIdx]._posSnapped);
                    var scale = OceanRenderer.Instance.CalcLodScale(lodIdx);
                    _renderMaterial[lodIdx].SetVector(sp_Scale, new Vector3(scale, 1f, scale));
                    _renderMaterial[lodIdx].SetVector(sp_JitterDiameters_CurrentFrameWeights, new Vector4(Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard));
                    _renderMaterial[lodIdx].SetMatrix(sp_MainCameraProjectionMatrix, GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix);
                    _renderMaterial[lodIdx].SetFloat(sp_SimDeltaTime, Time.deltaTime);

                    // compute which lod data we are sampling previous frame shadows from. if a scale change has happened this can be any lod up or down the chain.
                    var srcDataIdx = lodIdx + ScaleDifferencePow2;
                    srcDataIdx = Mathf.Clamp(srcDataIdx, 0, lt.LodCount - 1);
                    _renderMaterial[lodIdx].SetInt(sp_LD_SliceIndex, lodIdx);
                    _renderMaterial[lodIdx].SetInt(sp_LD_SliceIndex_Source, srcDataIdx);
                    _renderMaterial[lodIdx].SetTexture(GetParamIdSampler(true), _sources);
                    _renderMaterial[lodIdx].SetBuffer(sp_cascadeDataSrc, OceanRenderer.Instance._bufCascadeDataSrc);

                    BufCopyShadowMap.Blit(Texture2D.blackTexture, _targets, _renderMaterial[lodIdx].material, -1, lodIdx);
                }

#if ENABLE_VR && ENABLE_VR_MODULE
                // Disable for XR SPI otherwise input will not have correct world position.
                if (XRSettings.enabled && XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced)
                {
                    BufCopyShadowMap.DisableShaderKeyword("STEREO_INSTANCING_ON");
                }
#endif

                // Process registered inputs.
                for (var lodIdx = lt.LodCount - 1; lodIdx >= 0; lodIdx--)
                {
                    BufCopyShadowMap.SetRenderTarget(_targets, _targets.depthBuffer, 0, CubemapFace.Unknown, lodIdx);
                    SubmitDraws(lodIdx, BufCopyShadowMap);
                }

#if ENABLE_VR && ENABLE_VR_MODULE
                // Restore XR SPI as we cannot rely on remaining pipeline to do it for us.
                if (XRSettings.enabled && XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced)
                {
                    BufCopyShadowMap.EnableShaderKeyword("STEREO_INSTANCING_ON");
                }
#endif

                // Set the target texture as to make sure we catch the 'pong' each frame
                Shader.SetGlobalTexture(GetParamIdSampler(), _targets);
            }
        }

        public void ValidateSourceData()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                // Don't validate when not in play mode in editor as shadows won't be updating.
                return;
            }
#endif

            foreach (var renderData in OceanRenderer.Instance._lodTransform._renderDataSource)
            {
                renderData.Validate(BuildCommandBufferBase._lastUpdateFrame - OceanRenderer.FrameCount, SimName);
            }
        }

        internal override void OnEnable()
        {
            base.OnEnable();

            RemoveCommandBuffers();

            if (RenderPipelineHelper.IsHighDefinition)
            {
#if CREST_HDRP
                SampleShadowsHDRP.Enable();
#endif
            }
            else if (RenderPipelineHelper.IsUniversal)
            {
#if CREST_URP
                SampleShadowsURP.Enable();
#endif
            }
        }

        internal override void OnDisable()
        {
            base.OnDisable();

            RemoveCommandBuffers();

            if (RenderPipelineHelper.IsHighDefinition)
            {
#if CREST_HDRP
                SampleShadowsHDRP.Disable();
#endif
            }
            else if (RenderPipelineHelper.IsUniversal)
            {
#if CREST_URP
                SampleShadowsURP.Disable();
#endif
            }
        }

        void RemoveCommandBuffers()
        {
            if (BufCopyShadowMap != null)
            {
                if (_mainLight)
                {
                    _mainLight.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, BufCopyShadowMap);
                }
                BufCopyShadowMap = null;
            }
        }

        readonly static string s_textureArrayName = "_LD_TexArray_Shadow";
        private static TextureArrayParamIds s_textureArrayParamIds = new TextureArrayParamIds(s_textureArrayName);
        public static int ParamIdSampler(bool sourceLod = false) => s_textureArrayParamIds.GetId(sourceLod);
        protected override int GetParamIdSampler(bool sourceLod = false) => ParamIdSampler(sourceLod);

        public static void Bind(IPropertyWrapper properties)
        {
            if (OceanRenderer.Instance._lodDataShadow != null)
            {
                properties.SetTexture(OceanRenderer.Instance._lodDataShadow.GetParamIdSampler(), OceanRenderer.Instance._lodDataShadow.DataTexture);
            }
            else
            {
                properties.SetTexture(ParamIdSampler(), s_nullTexture);
            }
        }

        public static void BindNullToGraphicsShaders() => Shader.SetGlobalTexture(ParamIdSampler(), s_nullTexture);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            s_textureArrayParamIds = new TextureArrayParamIds(s_textureArrayName);
        }
    }
}
