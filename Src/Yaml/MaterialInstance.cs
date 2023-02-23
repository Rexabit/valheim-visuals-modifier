using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    [Serializable]
    public class MaterialInstance
    {
        public string Name;
        public string Original;
        public MaterialData MaterialData;
    }

    public class MaterialProperties
    {
        public Dictionary<string, Color> Colors;
        public Dictionary<string, float> Floats;
    }
}
