using System.Runtime.InteropServices;
using UnityEngine;

namespace VisualsModifier
{
    public class RendererMaterialEffect : IRendererEffect
    {
        public Material Value { get; set; }

        public RendererMaterialEffect(Material material)
        {
            Value = material;
        }

        public void Apply(Renderer r)
        {
            if (r.sharedMaterials.Length > 1)
            {
                Material[] materials = r.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = Value;
                }

                r.sharedMaterials = materials;
            } else
            {
                r.sharedMaterial = Value;
            }
        }
    }
}
