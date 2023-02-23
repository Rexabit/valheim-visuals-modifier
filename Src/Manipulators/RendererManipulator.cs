using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    class RendererManipulator : IManipulator
    {
        List<IRendererEffect> effects = new List<IRendererEffect>();

        public RendererManipulator(ShaderData data)
        {
            if (data.MaterialInstances != null)
            {
                AddValue(new RendererMaterialsEffect(data.MaterialInstances));
            } else if (data.MaterialInstance != null)
            {
                AddValue(new RendererMaterialEffect(data.MaterialInstance));
            }

            if (data.Name != null && data.Name != "")
            {
                AddValue(new RendererShaderEffect(data.Name));
            }
        }

        public void Invoke(Renderer smr, GameObject _prefab)
        {
            effects.ForEach(e => {
                e.Apply(smr);
            });
        }

        public void Invoke(Material m, GameObject prefab)
        {
            throw new System.NotImplementedException();
        }

        public void AddValue<IRendererEffect>(IRendererEffect e)
        {
            if (e != null)
            {
                effects.Add((VisualsModifier.IRendererEffect) e);
            }
        }
    }
}
