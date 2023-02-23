using UnityEngine;

namespace VisualsModifier
{
    public class RendererShaderEffect : IRendererEffect
    {
        public string Value { get; set; }
        private Shader Shader { get; set; }

        public RendererShaderEffect(string shader)
        {
            Value = shader;
            Shader = Shader.Find(shader);
        }

        public void Apply(Renderer r)
        {
            foreach (Material m in r.sharedMaterials)
            {
                m.shader = Shader;
            }
        }
    }
}
