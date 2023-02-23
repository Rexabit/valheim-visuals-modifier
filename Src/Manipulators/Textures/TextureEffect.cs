using UnityEngine;

namespace VisualsModifier
{
    public abstract class TextureEffect<T>
    {
        public T Value { get; set; }
        public string Name { get; set; }

        protected Texture2D Cache { get; set; }

        public TextureEffect(string name, T value)
        {
            Name = name;
            Value = value;
        }

        public Texture2D GetTexture(Material m)
        {
            if (!Cache)
            {
                Cache = (Texture2D)m.GetTexture(Name);
            }

            return Cache;
        }

        public Texture2D GetTexture(MaterialPropertyBlock m)
        {
            if (!Cache)
            {
                Cache = (Texture2D)m.GetTexture(Name);
            }

            return Cache;
        }
    }
}
