using System;
using UnityEngine;

namespace VisualsModifier
{
    [Serializable]
    public class RealtimeFloatEffect : RealtimeEffect<float>
    {
        public RealtimeFloatEffect(string name, float value) : base(name, value) { }

        public override void Apply(MaterialPropertyBlock mpb, Material m, RealtimeEffectData context, float ratio)
        {
            if (!Cached)
            {
                Cache = m.GetFloat(Name);
                Cached = true;
            }

            mpb.SetFloat(Name, Mathf.Lerp(Cache, Value, ratio));
        }
    }
}
