using System;
using UnityEngine;

namespace VisualsModifier
{
    [Serializable]
    public abstract class RealtimeEffect<T> : IRealtimeEffect
    {
        public string Name { get; set; }
        public T Value { get; set; }
        protected T Cache = default(T);
        protected bool Cached;

        public RealtimeEffect(string name, T value)
        {
            Name = name;
            Value = value;
            Cached = false;
        }

        public void Restore(T value)
        {
            Cache = value;
        }

        public abstract void Apply(MaterialPropertyBlock mpb, Material m, RealtimeEffectData context, float ratio);
    }
}
