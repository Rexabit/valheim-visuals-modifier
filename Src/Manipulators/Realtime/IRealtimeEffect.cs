using UnityEngine;

namespace VisualsModifier
{
    public interface IRealtimeEffect
    {
        public void Apply(MaterialPropertyBlock mpb, Material m, RealtimeEffectData data, float ratio);
    }
}