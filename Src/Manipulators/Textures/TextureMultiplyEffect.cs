using UnityEngine;

namespace VisualsModifier
{
    public class TextureMultiplyEffect : TextureEffect<Color>, ITextureEffect
    {
        public TextureMultiplyEffect(string name, Color value) : base(name, value) { }

        public void Apply(Material m, Texture2D tex = null)
        {
            if (tex == null)
            {
                tex = GetTexture(m);
            }

            if (tex != null)
            {
                if (m.HasProperty(this.Name))
                {
                    m.SetTexture(this.Name, Colour.AsMultiply(tex, this.Value));
                }
            }
        }

        public void Apply(MaterialPropertyBlock m, Texture2D tex = null)
        {
            if (tex == null)
            {
                tex = GetTexture(m);
            }

            if (tex != null)
            {
                m.SetTexture(this.Name, Colour.AsMultiply(tex, this.Value));
            }
        }
    }
}
