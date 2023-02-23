using UnityEngine;

namespace VisualsModifier
{
    public class RendererMaterialsEffect : IRendererEffect
    {
        public Material[] Value { get; set; }

        public RendererMaterialsEffect(Material[] materials)
        {
            Value = materials;
        }

        public void Apply(Renderer r)
        {
            if (r.sharedMaterials.Length > 1)
            {
                r.sharedMaterials = Value;
            }
            else
            {
                r.sharedMaterial = Value[0];
            }
        }
    }
}
