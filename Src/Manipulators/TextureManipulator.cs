using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    public class TextureManipulator: IManipulator
    {
        private List<ITextureEffect> effects = new();
        private Texture2D _texture;
        public TextureManipulator(TextureData data, Texture2D texture = null)
        {
            if (texture)
            {
                _texture = texture;
            }

            switch (data.Effect)
            {
                case TextureEffect.Multiply:
                    AddValue(new TextureMultiplyEffect(data.Name, data.Colors[0]));
                    break;
                case TextureEffect.Screen:
                    AddValue(new TextureScreenEffect(data.Name, data.Colors[0]));
                    break;
                case TextureEffect.Edge:
                    AddValue(new TextureToonEffect(data.Name, data.Colors.ToArray()));
                    break;
            }
        }

        public void AddValue<ITextureEffect>(ITextureEffect value)
        {
            if (value != null)
            {
                effects.Add((VisualsModifier.ITextureEffect)value);
            }
        }

        public void Invoke(Renderer smr, GameObject _prefab)
        {
            effects.ForEach(e => { 
                foreach (Material m in smr.sharedMaterials)
                {
                    e.Apply(m, _texture);
                }
            });
        }

        public void Invoke(Material m, GameObject prefab)
        {
            effects.ForEach(e => { e.Apply(m, _texture); });
        }
    }
}
