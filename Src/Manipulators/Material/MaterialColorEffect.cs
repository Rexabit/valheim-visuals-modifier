using UnityEngine;

namespace VisualsModifier
{
    public class MaterialColorEffect: MaterialEffect<Color>, IMaterialEffect
    {
        public MaterialColorEffect(string name, Color value) : base(name, value) { }

        public void Apply(Material m) { m.SetColor(Name, Value); }
        public void Apply(MaterialPropertyBlock m) { m.SetColor(Name, Value); }
    }
}
