using System.Collections.Generic;
using UnityEngine;

namespace VisualsModifier
{
    public static class MaterialManager
    {
        private static Dictionary<string, Material> _materials = new();
        private static Dictionary<string, Material> _shared = new();

        public static Material RegisterMaterial(string name, Material original)
        {
            if (_materials.ContainsKey(name))
            {
                return _materials[name];
            }

            // Create the material and set the name so we can easily look it up.
            Material material = Material.Instantiate(original);
            material.name = name;

            _materials[name] = material;

            return material;
        }

        /// <summary>
        /// Clone all of the materials for a group of renderers.
        /// </summary>
        /// <param name="name">The name of the prefab or game object at the root</param>
        /// <param name="renderers">The array of renderers in which to clone materials</param>
        public static void Clone(string name, Renderer[] renderers)
        {
            Dictionary<string, bool> _processed = new();

            foreach (Renderer r in renderers)
            {
                bool changed = false;

                if (r == null)
                {
                    continue;
                }

                if (_processed.ContainsKey(r.name))
                {
                    continue;
                }

                _processed[r.name] = true;

                if (r.sharedMaterials == null)
                {
                    continue;
                }

                List<Material> materials = new List<Material>();

                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    Material material = r.sharedMaterials[i];

                    if (material == null)
                    {
                        continue;
                    }

                    string uniqueMaterial = name + (material.name != null ? material.name : "Unknown");

                    // Unique material piece
                    if (!_shared.ContainsKey(uniqueMaterial) && !_shared.ContainsKey(material.name))
                    {
                        Material clone = Material.Instantiate(material);
                        clone.name = uniqueMaterial;

                        _shared[uniqueMaterial] = clone;

                        materials.Add(clone);

                        changed = true;
                    } else if (_shared.ContainsKey(material.name)) {
                        materials.Add(_shared[material.name]);

                        changed = true;
                    } else
                    {
                        materials.Add(_shared[uniqueMaterial]);

                        changed = true;
                    }
                }

                if (changed)
                {
                    r.sharedMaterials = materials.ToArray();
                }
            }
        }

        public static void Clear()
        {
            foreach (Material m in _materials.Values)
            {
                Object.DestroyImmediate(m);
            }

            _materials.Clear();
        }

        public static Material Get(string name)
        {
            return _materials[name];
        }

        public static bool Contains(string name)
        {
            return _materials.ContainsKey(name);
        }
    }
}