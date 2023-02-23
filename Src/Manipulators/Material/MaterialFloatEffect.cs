using UnityEngine;

namespace VisualsModifier
{
    public class MaterialFloatEffect : MaterialEffect<float>, IMaterialEffect
    {
        public MaterialFloatEffect(string name, float value) : base(name, value) { }

        public void Apply(Material m) { m.SetFloat(Name, Value); }
        public void Apply(MaterialPropertyBlock m) { m.SetFloat(Name, Value); }
    }
}
