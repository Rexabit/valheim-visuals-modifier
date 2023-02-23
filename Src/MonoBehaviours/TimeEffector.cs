using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    public class TimeEffector : MonoBehaviour, IRealtimeEffector
    {
        private const int UPDATE_INTERVAL = 15;

        private Renderer _renderer = null;
        private MaterialPropertyBlock _block = null;

        private List<IRealtimeEffect> _effects = new List<IRealtimeEffect>();

        public RealtimeEffectData _effectContext;

        void Awake()
        {
            _block = new MaterialPropertyBlock();
            _renderer = GetComponentInChildren<Renderer>();

            if (_renderer == null)
            {
                Debug.LogError("TimeEffector: Failed to retrieve renderer");
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

            Run();
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

        private void Run()
        {
            try
            {
                if (_renderer == null)
                {
                    Debug.LogError("TimeEffector: Failed to retrieve renderer");
                }

                if (_renderer.sharedMaterial == null)
                {
                    Debug.LogError("TimeEffector: No material in renderer");
                }

                ValheimTime vt = ValheimTime.Get();

                if (_effectContext == null) { Debug.LogError("TimeEffector: Bad context"); }
                if (_effectContext.Trigger == null) { Debug.LogError("TimeEffector: Bad Trigger"); }
                if (_effectContext.Trigger.Time == null) {  Debug.LogError("TimeEffector: Bad Time"); }
                if (_effectContext.Trigger.TimeSpan == null) { Debug.LogError("TimeEffector: Bad Time Span"); }

                float ratio = ValheimTime.RatioUntilTime(vt, _effectContext.Trigger.Time, _effectContext.Trigger.TimeSpan);

                //_renderer.GetPropertyBlock(_block);
                _effects.ForEach(e => { e.Apply(_block, _renderer.sharedMaterial, _effectContext, ratio); });
                _renderer.SetPropertyBlock(_block);
            } catch (Exception ex)
            {
                Debug.Log(ex.StackTrace);
            }
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
            Material[] m = this.GetComponent<Renderer>().sharedMaterials;

            for (int i = 0; i < m.Length; i++)
            {
                m[i].SetInt("_Item", item);
            }
        }
    }
}