// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

namespace Crest
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Underwater Renderer. If a camera needs to go underwater it needs to have this script attached. This adds
    /// fullscreen passes and should only be used if necessary. This effect disables itself when camera is not close to
    /// the water volume.
    ///
    /// For convenience, all shader material settings are copied from the main ocean shader.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu(Internal.Constants.MENU_PREFIX_SCRIPTS + "Underwater Renderer")]
    [HelpURL(Internal.Constants.HELP_URL_BASE_USER + "underwater.html" + Internal.Constants.HELP_URL_RP)]
    public partial class UnderwaterRenderer : MonoBehaviour
    {
        /// <summary>
        /// The version of this asset. Can be used to migrate across versions. This value should
        /// only be changed when the editor upgrades the version.
        /// </summary>
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        // This adds an offset to the cascade index when sampling ocean data, in effect smoothing/blurring it. Default
        // to shifting the maximum amount (shift from lod 0 to penultimate lod - dont use last lod as it cross-fades
        // data in/out), as more filtering was better in testing.
        [SerializeField, Range(0, LodDataMgr.MAX_LOD_COUNT - 2)]
        [Tooltip("How much to smooth ocean data such as water depth, light scattering, shadowing. Helps to smooth flickering that can occur under camera motion.")]
        internal int _filterOceanData = LodDataMgr.MAX_LOD_COUNT - 2;

        [SerializeField]
        [Tooltip("Add a meniscus to the boundary between water and air.")]
        internal bool _meniscus = true;


        [Header("Advanced")]

        [SerializeField]
        [Tooltip("Copying params each frame ensures underwater appearance stays consistent with ocean material params. Has a small overhead so should be disabled if not needed.")]
        internal bool _copyOceanMaterialParamsEachFrame = true;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Adjusts the far plane for horizon line calculation. Helps with horizon line issue.")]
        internal float _farPlaneMultiplier = 0.68f;

        [Space(10)]

        [SerializeField]
        internal DebugFields _debug = new DebugFields();
        [System.Serializable]
        public class DebugFields
        {
            public bool _viewOceanMask = false;
            public bool _disableOceanMask = false;
            public bool _disableHeightAboveWaterOptimization = false;
        }

        Camera _camera;
        bool _firstRender = true;
        // XR MP will create two instances of this class so it needs to be static to track the pass/eye.
        internal static int s_xrPassIndex = -1;

        // Use instance to denote whether this is active or not. Only one camera is supported.
        public static UnderwaterRenderer Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStatics()
        {
            Instance = null;
            s_xrPassIndex = -1;
        }

        internal bool IsActive
        {
            get
            {
                if (OceanRenderer.Instance == null)
                {
                    return false;
                }

                if (!_debug._disableHeightAboveWaterOptimization && OceanRenderer.Instance.ViewerHeightAboveWater > 2f)
                {
                    return false;
                }

                return true;
            }
        }

        void OnEnable()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

#if UNITY_EDITOR
            Validate(OceanRenderer.Instance, ValidatedHelper.DebugLog);
#endif

            // Setup here because it is the same across pipelines.
            if (_cameraFrustumPlanes == null)
            {
                _cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(_camera);
            }

            if (RenderPipelineHelper.IsUniversal)
            {
#if CREST_URP
                UnderwaterMaskPassURP.Enable(this);
                UnderwaterEffectPassURP.Enable(this);
#endif
            }
            else if (RenderPipelineHelper.IsHighDefinition)
            {
#if CREST_HDRP
                UnderwaterMaskPassHDRP.Enable(this);
                UnderwaterEffectPassHDRP.Enable(this);
#endif
            }
            else
            {
                Enable();
            }

            Instance = this;
        }

        void OnDisable()
        {
            if (RenderPipelineHelper.IsUniversal)
            {
#if CREST_URP
                UnderwaterMaskPassURP.Disable();
                UnderwaterEffectPassURP.Disable();
#endif
            }
            else if (RenderPipelineHelper.IsHighDefinition)
            {
#if CREST_HDRP
                UnderwaterMaskPassHDRP.Disable();
                UnderwaterEffectPassHDRP.Disable();
#endif
            }
            else
            {
                Disable();
            }

            Instance = null;
        }

        void Enable()
        {
            SetupOceanMask();
            SetupUnderwaterEffect();
            _camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, _underwaterEffectCommandBuffer);
            _camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _oceanMaskCommandBuffer);
        }

        void Disable()
        {
            _camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, _underwaterEffectCommandBuffer);
            _camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _oceanMaskCommandBuffer);
        }

        void OnPreRender()
        {
            if (!IsActive)
            {
                OnDisable();
                return;
            }

            if (GL.wireframe)
            {
                OnDisable();
                return;
            }

            if (Instance == null)
            {
                OnEnable();
            }

            XRHelpers.Update(_camera);
            XRHelpers.UpdatePassIndex(ref s_xrPassIndex);

            OnPreRenderOceanMask();
            OnPreRenderUnderwaterEffect();

            _firstRender = false;
        }
    }

#if UNITY_EDITOR
    public partial class UnderwaterRenderer : IValidated
    {
        public bool Validate(OceanRenderer ocean, ValidatedHelper.ShowMessage showMessage)
        {
            var isValid = true;

#if CREST_HDRP
            if (RenderPipelineHelper.IsHighDefinition && XRGraphics.enabled && _camera.usePhysicalProperties)
            {
                showMessage
                (
                    "Underwater Renderer is currently not compatible with physical cameras in XR for HDRP. " +
                    "It could be a Unity bug which Unity is looking into.",
                    "Disable <i>Link FOV to Physical Camera</i> on the <i>Camera</i> component.",
                    ValidatedHelper.MessageType.Error, _camera
                );
            }
#endif

            return isValid;
        }
    }

    [CustomEditor(typeof(UnderwaterRenderer)), CanEditMultipleObjects]
    public class UnderwaterRendererEditor : ValidatedEditor { }
#endif
}
