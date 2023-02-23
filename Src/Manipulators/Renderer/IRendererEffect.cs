using UnityEngine;

namespace VisualsModifier
{
    public interface IRendererEffect
    {
        void Apply(Renderer r);
    }
}
