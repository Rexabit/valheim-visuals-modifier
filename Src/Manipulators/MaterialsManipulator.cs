using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    class MaterialsManipulator : IManipulator
    {
        List<List<IMaterialEffect>> effects = new List<List<IMaterialEffect>>();

        public MaterialsManipulator(MaterialData[] materialChanges)
        {
            for (int i = 0; i < materialChanges.Length; i++)
            {
                MaterialData data = materialChanges[i];

                effects.Add(new List<IMaterialEffect>());

                if (data.Colors != null)
                {
                    foreach (KeyValuePair<string, Color> entry in data.Colors)
                    {
                        effects[i].Add(new MaterialColorEffect(entry.Key, entry.Value));
                    }
                }

                if (data.Floats != null)
                {
                    foreach (KeyValuePair<string, float> entry in data.Floats)
                    {
                        effects[i].Add(new MaterialFloatEffect(entry.Key, entry.Value));
                    }
                }
            }
        }

        public void Invoke(Renderer smr, GameObject prefab)
        {
            for (int i = 0; i < smr.sharedMaterials.Length && i < effects.Count; i++)
            {
                Material m = smr.sharedMaterials[i];

                if (effects[i] != null)
                {
                    effects[i].ForEach(e => {
                        e.Apply(m);
                    });
                }
            }
        }

        public void Invoke(Material m, GameObject prefab)
        {
        }
    }
}
