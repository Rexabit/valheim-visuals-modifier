using UnityEngine;

namespace VisualsModifier
{
    public class TextureToonEffect : TextureEffect<Color[]>,  ITextureEffect
    {
        public TextureToonEffect(string name, Color[] value) : base(name, value) { }

        public void Apply(Material m, Texture2D tex = null)
        {
            if (tex == null)
            {
                tex = GetTexture(m);
            }

            if (tex != null)
            {
                if (this.Value.Length >= 3)
                {
                    m.SetTexture(this.Name, Colour.AsCell(tex, this.Value[0], this.Value[1], this.Value[2]));
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
                m.SetTexture(this.Name, Colour.AsCell(tex, this.Value[0], this.Value[1], this.Value[2]));
            }
        }
    }
}
