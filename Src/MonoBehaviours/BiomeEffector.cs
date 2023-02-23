using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    public class BiomeEffector : MonoBehaviour, IRealtimeEffector
    {
        private const int UPDATE_INTERVAL = 1;
        private const int TRANSITION_DURATION = 1;

        private Renderer _renderer = null;
        private MaterialPropertyBlock _block = null;
        private List<IRealtimeEffect> _effects = new();

        private float _currentRatio = 0.0f;

        private RealtimeEffectData _effectContext;

        void Awake()
        {
            _block = new MaterialPropertyBlock();
            _renderer = GetComponentInChildren<Renderer>();

            if (_renderer == null)
            {
                Debug.LogError("BiomeEffector: Failed to retrieve renderer");
            }

            int item = _renderer.sharedMaterial.GetInt("_Item");

            VisualData data = VisualController.GetVisualByIndex(item);

            if (data == null)
            {
                return;
            }

            if (data.Effect != null)
            {
                ApplyContext(data.Effect);
            }
        }

        void Start()
        {
            if (_effectContext == null)
            {
                return;
            }
        }

        void Run()
        {
            ValheimTime vt = ValheimTime.Get();

            _currentRatio = Mathf.MoveTowards(_currentRatio, GetRatio(), (TRANSITION_DURATION * UPDATE_INTERVAL) * Time.deltaTime);

            _renderer.GetPropertyBlock(_block);
            _effects.ForEach(e => { e.Apply(_block, _renderer.sharedMaterial, _effectContext, _currentRatio); });
            _renderer.SetPropertyBlock(_block);
        }

        void LateUpdate()
        {
            if (_effectContext == null)
            {
                return;
            }

            // Update once every 30 updates, no point updating more often
            if (Time.frameCount % UPDATE_INTERVAL != 0)
            {
                return;
            }

            Run();
        }

        private float GetRatio()
        {
            Heightmap.Biome biome = EnvMan.instance.GetBiome();

            return biome == _effectContext.Trigger.Biome ? 1.0f : 0.0f;
        }

        public void ApplyContext(RealtimeEffectData data)
        {
            _effectContext = data;

            try
            {
                if (data.Material.Colors != null && data.Material.Colors.Count > 0)
                {
                    foreach (KeyValuePair<string, Color> entity in data.Material.Colors)
                    {
                        _effects.Add(new RealtimeColorEffect(entity.Key, entity.Value));
                    }
                }

                if (data.Material.Floats != null && data.Material.Floats.Count > 0)
                {
                    foreach (KeyValuePair<string, float> entity in data.Material.Floats)
                    {
                        _effects.Add(new RealtimeFloatEffect(entity.Key, entity.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError(ex.InnerException);
            }

        }
        public void SetVisuals(int item)
        {
            GetComponent<Renderer>().sharedMaterial.SetInt("_Item", item);
        }
    }
}