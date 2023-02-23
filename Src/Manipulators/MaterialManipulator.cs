using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    class MaterialManipulator : IManipulator
    {
        List<IMaterialEffect> properties = new List<IMaterialEffect>();

        public MaterialManipulator(MaterialData data)
        {
            if (data.Colors != null)
            {
                foreach (KeyValuePair<string, Color> entry in data.Colors)
                {
                    AddValue(new MaterialColorEffect(entry.Key, entry.Value));
                }
            }
 
            if (data.Floats != null)
            {
                foreach (KeyValuePair<string, float> entry in data.Floats)
                {
                    AddValue(new MaterialFloatEffect(entry.Key, entry.Value));
                }
            }
        }

        public void Invoke(Renderer smr, GameObject prefab)
        {
            properties.ForEach(p => {                 
                foreach (Material m in smr.sharedMaterials)
                {
                    if (m)
                    {
                        p.Apply(m);
                    }
                }
            });  
        }

        public void Invoke(Material m, GameObject prefab)
        {
            properties.ForEach(e => {
                if (m)
                {
                    e.Apply(m);
                }
            });
        }

        public void AddValue<IMaterialEffect>(IMaterialEffect p)
        {
            if (p != null)
            {
                properties.Add((VisualsModifier.IMaterialEffect)p);
            }
        }
    }
}
