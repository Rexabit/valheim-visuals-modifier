using System;
using UnityEngine;

namespace VisualsModifier
{
    [Serializable]
    public class RealtimeColorEffect : RealtimeEffect<Color>
    {
        public RealtimeColorEffect(string name, Color value) : base(name, value) { }

        public override void Apply(MaterialPropertyBlock mpb, Material m, RealtimeEffectData context, float ratio)
        {
            if (!Cached)
            {
                Cache = m.GetColor(Name);
                Cached = true;
            }

            mpb.SetColor(Name, Color.Lerp(Cache, Value, ratio));
        }
    }
}