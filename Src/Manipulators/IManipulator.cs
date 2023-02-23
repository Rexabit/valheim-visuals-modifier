using UnityEngine;

namespace VisualsModifier
{
    public interface IManipulator
    {
        void Invoke(Renderer m, GameObject prefab);

        void Invoke(Material m, GameObject prefab);
    }
}
