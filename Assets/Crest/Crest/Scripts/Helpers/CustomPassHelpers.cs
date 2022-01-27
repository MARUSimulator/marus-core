// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

#if CREST_HDRP

namespace Crest
{
    using UnityEngine;
    using UnityEngine.Rendering.HighDefinition;

    public static class CustomPassHelpers
    {
        public static void CreateOrUpdate<T>(ref GameObject gameObject, string name, CustomPassInjectionPoint injectionPoint) where T : CustomPass, new()
        {
            // Find the existing custom pass volume.
            // During recompiles, the reference will be lost so we need to find the game object. It could be limited to
            // the editor if it is safe to do so. The last thing we want is leaking objects.
            if (gameObject == null)
            {
                var transform = OceanRenderer.Instance.transform.Find(name);
                if (transform != null)
                {
                    gameObject = transform.gameObject;
                }
            }

            // Create or update the custom pass volume.
            if (gameObject == null)
            {
                gameObject = new GameObject()
                {
                    name = name,
                    hideFlags = OceanRenderer.Instance._hideOceanTileGameObjects
                        ? HideFlags.HideAndDontSave : HideFlags.DontSave,
                };
                // Place the custom pass under the ocean renderer since it is easier to find later. Transform.Find can
                // find inactive game objects unlike GameObject.Find.
                gameObject.transform.parent = OceanRenderer.Instance.transform;
            }
            else
            {
                gameObject.hideFlags = OceanRenderer.Instance._hideOceanTileGameObjects
                        ? HideFlags.HideAndDontSave : HideFlags.DontSave;
                gameObject.SetActive(true);
            }

            // Create the custom pass volume if it does not exist.
            if (!gameObject.TryGetComponent<CustomPassVolume>(out var _))
            {
                // It appears that this is currently the only way to add a custom pass.
                var volume = gameObject.AddComponent<CustomPassVolume>();
                volume.injectionPoint = injectionPoint;
                volume.isGlobal = true;
                volume.customPasses.Add(new T()
                {
                    name = $"Crest {name}",
                    targetColorBuffer = CustomPass.TargetBuffer.None,
                    targetDepthBuffer = CustomPass.TargetBuffer.None,
                });
            }
        }
    }
}

#endif
