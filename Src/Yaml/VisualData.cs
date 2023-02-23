using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace VisualsModifier
{
    [Serializable]
    public class VisualData
    {
        public string PrefabName;
        public bool Base = false;
        public ShaderData Shader = null;
        public MaterialData Material = null;
        public MaterialData[] Materials = null;
        public MaterialData Particle = null;
        public TextureData Texture = null;
        public LightData Light = null;
        public RealtimeEffectData Effect = null;
        public Vector3 Icon = Vector3.zero;
    }
    public class LightData
    {
        public Color Color;
        public float? Range;
    }

    [Serializable]
    public class ShaderData
    {
        public string Material;
        public string[] Materials;
        public string Name;
        [YamlIgnore]
        public Material MaterialInstance;
        [YamlIgnore]
        public Material[] MaterialInstances;
    }

    [Serializable]
    public class MaterialData
    {
        public Dictionary<string, Color> Colors;
        public Dictionary<string, float> Floats;
    }

    [Serializable]
    public enum RealtimeEffectType
    {
        Proximity,
        Time,
        Biome
    }

    [Serializable]
    public class RealtimeEffectTrigger
    {
        public ValheimTime Time;
        public ValheimTime TimeSpan;
        public List<string> Entities;
        public float Radius;
        public Heightmap.Biome Biome;
    }

    [Serializable]
    public class RealtimeEffectData
    {
        public RealtimeEffectType Type;
        public RealtimeEffectTrigger Trigger;
        public MaterialData Material;
    }
}