using System;
using UnityEngine;

namespace VisualsModifier
{
    public class TextureScreenEffect : TextureEffect<Color>, ITextureEffect
    {
        public TextureScreenEffect(string name, Color value) : base(name, value) { }

        public void Apply(Material m, Texture2D tex = null)
        {
            string rootName = m.name.Split(new string[] { "(Clone)" }, StringSplitOptions.None)[0];

            if (VisualController.ProcessedTextures.ContainsKey(m.name)) {
                return;
            }

            if (!m.HasProperty(Name))
            {
                return;
            }

            if (tex == null)
            {
                tex = GetTexture(m);
            }

            if (tex != null)
            {
                if (m.HasProperty(this.Name))
                {
                    m.SetTexture(this.Name, Colour.AsScreen(tex, this.Value));
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
                m.SetTexture(this.Name, Colour.AsScreen(tex, this.Value));
            }
        }
    }
}