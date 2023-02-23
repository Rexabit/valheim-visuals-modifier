using UnityEngine;

namespace VisualsModifier
{
    public interface ITextureEffect
    {
        /// <summary>
        /// Applies the changes directly to the material
        /// </summary>
        /// <param name="m">The material to apply the changes to</param>
        void Apply(Material m, Texture2D t = null);

        /// <summary>
        /// Applies the material effect changes to the property block
        /// </summary>
        /// <param name="m">The material property block to apply the changes to</param>
        void Apply(MaterialPropertyBlock m, Texture2D t = null);
    }
}
