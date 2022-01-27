// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using UnityEngine;

public class SubmarineParticles : MonoBehaviour
{
    [SerializeField]
    private float _maxVisibleDistance = 15f;

    private Submarine _submarine;
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _submarine = GetComponentInParent<Submarine>();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if ((_particleSystem != null) || (_submarine != null))
        {
            float subSpeed = Mathf.Clamp(_submarine._submarineSpeed, 0f, 1f);
            var emission = _particleSystem.emission;
            emission.rateOverTime = Mathf.Lerp(0, 200, subSpeed);

            // Fade when camera not close to emulate water fogging. This would be cleaner in a VFX graph.
            if (SubmarineCamera.Instance != null)
            {
                float dist = (transform.position - SubmarineCamera.Instance.transform.position).magnitude;
                var main = _particleSystem.main;
                var mmc = main.startColor;
                var col = mmc.color;
                col.a = 0.1f * Mathf.Clamp01(1f - dist / _maxVisibleDistance);
                main.startColor = new ParticleSystem.MinMaxGradient(col);
            }
        }
    }
}
