using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using BepInEx;

namespace VisualsModifier
{
    public static class VisualController
    {
        public static Dictionary<string, int> _visualsByName = new Dictionary<string, int>();
        public static Dictionary<int, int> _visualsByHash = new Dictionary<int, int>();
        public static List<VisualData> _visuals = new List<VisualData>();

        public static Dictionary<string, Texture2D> _initialTextures = new();
        public static Dictionary<string, Material> _initialMaterials = new();
        public static Dictionary<string, bool> ProcessedTextures = new();
        public static Dictionary<string, Material> Materials = new();

        private static ISerializer _serializer;
        private static IDeserializer _deserializer;

        static VisualController()
        {
            ColorConverter cc = new ColorConverter();
            ValheimTimeConverter vtc = new ValheimTimeConverter();

            _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(cc)
            .WithTypeConverter(vtc)
            .Build();

            _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(cc)
            .WithTypeConverter(vtc)
            .Build();
        }

        public static void Reload(List<VisualData> visuals)
        {
            foreach(VisualData visual in visuals)
            {
                Add(visual.PrefabName, visual);
            }
        }

        public static void Add(string prefabName, VisualData data)
        {
            int hash = prefabName.GetStableHashCode();

            if (_visualsByName.ContainsKey(prefabName)) {
                int index = GetVisualIndex(prefabName);

                _visuals[index] = data;
                _visualsByHash[hash] = index;
                _visualsByName[prefabName] = index;
            } else {
                _visuals.Add(data);
                _visualsByHash.Add(hash, _visuals.Count - 1);
                _visualsByName.Add(prefabName, _visuals.Count - 1);
            }
        }

        public static VisualData Import(string file)
        {
            try
            {
                VisualData data = _deserializer.Deserialize<VisualData>(File.ReadAllText(file));

                Add(data.PrefabName, data);

                return data;
            } catch(System.Exception e)
            {
                Debug.LogError($"Found {Plugin.PluginName} config error in file {file}.\n{e.Message + (e.InnerException != null ? ": " + e.InnerException.Message : "")}");
                return null;
            }
        }

        public static void Export(VisualData visual)
        {
            string contents = _serializer.Serialize(visual);
            string storage  = Path.Combine(Paths.ConfigPath, "Visuals");

            if (!Directory.Exists(storage))
            {
                Directory.CreateDirectory(storage);
            }
            
            File.WriteAllText(Path.Combine(storage, "Visual_" + visual.PrefabName + ".yml"), contents);
        }

        public static void Export(DescriptorData data)
        {
            string contents = _serializer.Serialize(data);
            string storage = Path.Combine(Paths.ConfigPath, "Visuals");

            if (!Directory.Exists(storage))
            {
                Directory.CreateDirectory(storage);
            }

            File.WriteAllText(Path.Combine(storage, "Describe_" + data.Name + ".yml"), contents);
        }

        public static DescriptorData Describe(string prefabName)
        {
            DescriptorData data = new DescriptorData() { Name = prefabName };

            // Retrieve the prefab
            GameObject item = ObjectDB.instance.GetItemPrefab(prefabName);

            if (!item)
            {
                return data;
            }

            // Fetch the skin
            Transform skin = item.transform.Find("attach_skin") ?? item.transform.Find("attach");

            if (skin)
            {
                Renderer[] skin_renderers = skin.GetComponentsInChildren<Renderer>(true);

                for (int i = 0; i < skin_renderers.Length; i++)
                {
                    RendererDescriptor rd = new RendererDescriptor() { Name = skin.name };

                    // Reference sharedMaterials as accessing 'materials' causes new instances
                    for (int j = 0; j < skin_renderers[i].sharedMaterials.Length; j++)
                    {
                        Material m = skin_renderers[i].sharedMaterials[j];
                        MaterialDescriptor md = new MaterialDescriptor() { Name = skin_renderers[i].sharedMaterials[j].name, Shader = m.shader.name };

                        int propertyCount = m.shader.GetPropertyCount();

                        for (int k = 0; k < propertyCount; k++)
                        {
                            ShaderPropertyType type = m.shader.GetPropertyType(k);
                            string name = m.shader.GetPropertyName(k);

                            MaterialPropertyDescriptor mpd = new MaterialPropertyDescriptor()
                            {
                                Name = name,
                                Type = type.ToString(),
                                Range = type == ShaderPropertyType.Range ? string.Format("{0} to {1}", m.shader.GetPropertyRangeLimits(k).x, m.shader.GetPropertyRangeLimits(k).y) : null,
                                Value = type == ShaderPropertyType.Color ? m.GetColor(name).ToString() :
                                        type == ShaderPropertyType.Range ? m.GetFloat(name).ToString() :
                                        type == ShaderPropertyType.Float ? m.GetFloat(name).ToString() :
                                        type == ShaderPropertyType.Vector ? m.GetVector(name).ToString() : null
                            };
                            
                            md.MaterialProperties.Add(mpd);
                        }

                        rd.Materials.Add(md);
                    }

                    data.Renderers.Add(rd);
                }
            }

            return data;
        }

        public static bool TryUpdateTexture(VisualData data, string prefabName, GameObject armor)
        {
            if (data.Texture == null)
            {
                return false;
            }

            // Get the base armor material
            ItemDrop id = armor.GetComponent<ItemDrop>();
            Material m = id ? id.m_itemData.m_shared.m_armorMaterial : null;

            if (!m)
            {
                return false;
            }

            try
            {
                string rootName = m.name.Split(new string[] { "(Clone)" }, System.StringSplitOptions.None)[0];

                if (!_initialMaterials.ContainsKey(rootName))
                {
                    _initialMaterials.Add(rootName, Material.Instantiate(m));
                }
                   
                m = _initialMaterials[rootName];

                if (!_initialTextures.ContainsKey(rootName))
                {
                    _initialTextures.Add(rootName, Colour.CloneTexture((Texture2D)m.GetTexture(data.Texture.Name)));
                }

                TextureManipulator tm = new TextureManipulator(data.Texture, _initialTextures[rootName]);

                tm.Invoke(m, armor);

                id.m_itemData.m_shared.m_armorMaterial = m;

                ProcessedTextures.Add(rootName, true);

                return true;
            } catch(System.Exception ex)
            {
                return false;
            }
        }

        public static Transform GetDropChild(GameObject item)
        {
            for (int i = 0; i < item.transform.childCount; i++)
            {
                Transform child = item.transform.GetChild(i);

                if (child.name.Contains("attach")) { continue; }

                return child;
            }

            return null;
        }

        public static void UpdateVisuals(string prefabName, ObjectDB instance)
        {
            VisualData data = VisualController.GetVisualByName(prefabName);
            GameObject item = instance.GetItemPrefab(prefabName);

            if (item == null)
            {
                return;
            }
            
            try
            {
                Transform skin_meshes   = item.transform.Find("attach_skin"); // Find Skinned Meshes
                Transform static_meshes = item.transform.Find("attach");      // Find Static Meshes
                Transform drop_meshes   = GetDropChild(item);                 // Find Drop Visual

                List<Renderer> renderers = new List<Renderer>();

                // Get renderers for each visual component
                Renderer[] skinRenderers = skin_meshes != null ? skin_meshes.GetComponentsInChildren<SkinnedMeshRenderer>(true) : null;
                Renderer[] dropRenderers = drop_meshes != null ? drop_meshes.GetComponentsInChildren<MeshRenderer>(true) : null;
                Renderer[] meshRenderers = static_meshes != null ? static_meshes.GetComponentsInChildren<MeshRenderer>(true) : null;

                if (skinRenderers != null) { renderers.AddRange(skinRenderers); }
                if (dropRenderers != null) { renderers.AddRange(dropRenderers); }
                if (meshRenderers != null) { renderers.AddRange(meshRenderers); }

                List<IManipulator> rendererChanges = VisualController.GetRendererChanges(prefabName);

                foreach (Renderer renderer in renderers)
                {
                    if (renderer.GetType() == typeof(ParticleSystemRenderer))
                    {
                        continue;
                    }

                    rendererChanges.ForEach(change => { change.Invoke(renderer, item); });
                }

                // Here we create new unique materials for the prefab if they haven't been created already.
                // Material Manager will not clone a meterial that has already been cloned.
                if (skinRenderers != null) { MaterialManager.Clone("skn_" + prefabName, skinRenderers); }
                if (meshRenderers != null) { MaterialManager.Clone("msh_" + prefabName, meshRenderers); }
                if (dropRenderers != null) { MaterialManager.Clone("drp_" + prefabName, dropRenderers); }
                
                List<IManipulator> materialChanges = VisualController.GetManipulations(prefabName);
                List<IManipulator> particleChanges = VisualController.GetParticleChanges(prefabName);

                if (TryUpdateTexture(data, prefabName, item))
                {
                    //Debug.Log("[VisualController] Updated base material for: " + prefabName);
                }

                for (int i = 0; i < renderers.Count; i++)
                {
                    if (renderers[i].GetType() == typeof(ParticleSystemRenderer))
                    {
                        particleChanges.ForEach(change => { change.Invoke(renderers[i], item); });
                    } else
                    {
                        materialChanges.ForEach(change => { change.Invoke(renderers[i], item); });
                    }                    
                }

                if (data.Light != null)
                {
                    Light l = item.GetComponentInChildren<Light>(true);
                    if (l != null)
                    {
                        if (data.Light.Color != null)
                        {
                            l.color = data.Light.Color;
                        }

                        if (data.Light.Range.HasValue)
                        {
                            l.range = data.Light.Range.Value;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[${Plugin.PluginName}]: Failed to update material - {e.Message}");
            }

            ItemDrop drop = item.GetComponent<ItemDrop>();

            if (drop)
            {
                UpdateIcon(drop, data.Icon);
            }
        }

        public static void UpdateIcon(ItemDrop item, Vector3 r)
        {
            const int layer = 30;

            Camera camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
            camera.backgroundColor = Color.clear;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.fieldOfView = 0.5f;
            camera.farClipPlane = 10000000;
            camera.cullingMask = 1 << layer;

            Light topLight = new GameObject("Light", typeof(Light)).GetComponent<Light>();
            topLight.transform.rotation = Quaternion.Euler(60, -5f, 0);
            topLight.type = LightType.Directional;
            topLight.cullingMask = 1 << layer;
            topLight.intensity = 0.7f;

            try
            {
                Rect rect = new(0, 0, 64, 64);
                Quaternion rotation = Quaternion.Euler(r.x, r.y, r.z);

                Transform target = item.transform.Find("attach"); // ?? item.transform.Find("attach_skin");

                if (!target)
                {
                    target = GetDropChild(item.gameObject);
                }

                GameObject visual = Object.Instantiate(target.gameObject, Vector3.zero, rotation);
                foreach (Transform child in visual.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = layer;
                }

                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                Vector3 min = renderers.Aggregate(Vector3.positiveInfinity, (cur, renderer) => renderer is ParticleSystemRenderer ? cur : Vector3.Min(cur, renderer.bounds.min));
                Vector3 max = renderers.Aggregate(Vector3.negativeInfinity, (cur, renderer) => renderer is ParticleSystemRenderer ? cur : Vector3.Max(cur, renderer.bounds.max));
                Vector3 size = max - min;

                camera.targetTexture = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);
                float zDist = Mathf.Max(size.x, size.y) * 1.05f / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad);
                Transform transform = camera.transform;
                transform.position = (min + max) / 2 + new Vector3(0, 0, -zDist);
                topLight.transform.position = transform.position + new Vector3(-2, 0.2f) / 3 * zDist;

                camera.Render();

                RenderTexture currentRenderTexture = RenderTexture.active;
                RenderTexture.active = camera.targetTexture;

                Texture2D texture = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                texture.ReadPixels(rect, 0, 0);
                texture.Apply();

                RenderTexture.active = currentRenderTexture;

                item.m_itemData.m_shared.m_icons = new[] { Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f)) };

                Object.DestroyImmediate(visual);
                camera.targetTexture.Release();

            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[${Plugin.PluginName}]: Failed to update icon - {ex.Message}");
            }
            finally
            {
                Object.Destroy(camera);
                Object.Destroy(topLight);
            }

        }

        public static int GetVisualIndex(string name)
        {
            return _visualsByName[name];
        }

        public static int GetVisualIndex(int hash)
        {
            return _visualsByHash[hash];
        }

        public static VisualData GetVisualByIndex(int index)
        {
            return _visuals[index];
        }

        public static VisualData GetVisualByName(string name)
        {
            if (_visualsByName.ContainsKey(name))
            {
                return _visuals[_visualsByName[name]];
            }

            return null;
        }

        public static VisualData GetVisualByHash(int hash)
        {
            if (_visualsByHash.ContainsKey(hash))
            {
                return _visuals[_visualsByHash[hash]];

            }
            return null;
        }

        public static void LoadVisuals()
        {
            ProcessedTextures.Clear();

            string storage = Path.Combine(Paths.ConfigPath, "Visuals");

            foreach (string file in Directory.GetFiles(storage, "Visual_*.yml", SearchOption.AllDirectories))
            {
                try
                {
                    Import(file);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[${Plugin.PluginName}]: Failed to load visual data from {file}");
                }
            }
        }

        public static VisualData LoadYaml(string yaml)
        {
            try
            {
                VisualData data = _deserializer.Deserialize<VisualData>(yaml);

                Add(data.PrefabName, data);

                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Found {Plugin.PluginName} config error in yaml.\n{e.Message + (e.InnerException != null ? ": " + e.InnerException.Message : "")}");
                return null;
            }
        }

        public static void LoadVisuals(Dictionary<string, string> data)
        {
            ProcessedTextures.Clear();

            foreach (KeyValuePair<string, string> pair in data)
            {
                LoadYaml(pair.Value);
            }
        }

        public static void LoadCustomMaterials()
        {
            //MaterialManager.Clear();
            LoadMaterials();

            foreach (string file in Directory.GetFiles(Paths.ConfigPath, "Material_*.yml", SearchOption.AllDirectories))
            {
                try
                {
                    MaterialInstance material = _deserializer.Deserialize<MaterialInstance>(File.ReadAllText(file));

                    if (!MaterialManager.Contains(material.Name))
                    {
                        MaterialManager.RegisterMaterial(material.Name, Materials[material.Original]);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[${Plugin.PluginName}]: Failed to load material data from {file}");
                }
            }
        }

        public static void LoadMaterials()
        {
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            foreach (Material m in materials)
            {
                Materials[m.name] = m;
            }
        }

        public static void LoadFromYaml(string yaml)
        {
            VisualData[] items = _deserializer.Deserialize<VisualData[]>(yaml);

            foreach(VisualData item in items)
            {
                Add(item.PrefabName, item);
            }
        }

        public static void Apply()
        {
            ObjectDB instance = ObjectDB.instance;

            _visuals.ForEach(action => { UpdateVisuals(action.PrefabName, instance); });
        }

        public static List<IManipulator> GetRendererChanges(string prefab)
        {
            List<IManipulator> manipulations = new List<IManipulator>();
            VisualData data = GetVisualByName(prefab);

            if (data != null && data.Shader != null)
            {
                if (data.Shader.Material != null && data.Shader.Material != "")
                {
                    if (Materials.ContainsKey(data.Shader.Material))
                    {
                        data.Shader.MaterialInstance = Materials[data.Shader.Material];
                    }
                }

                if (data.Shader.Materials != null && data.Shader.Materials.Length > 0)
                {
                    data.Shader.MaterialInstances = new Material[data.Shader.Materials.Length];

                    for (int i = 0; i < data.Shader.MaterialInstances.Length; i++) {
                        data.Shader.MaterialInstances[i] = Materials[data.Shader.Materials[i]];
                    }
                }

                manipulations.Add(new RendererManipulator(data.Shader));
            }

            return manipulations;
        }

        public static List<IManipulator> GetManipulations(string prefab)
        {
            List<IManipulator> manipulations = new List<IManipulator>();
            VisualData data = GetVisualByName(prefab);
            
            if (data == null)
            {
                return manipulations;
            }

            if (data.Material != null)
            {
                manipulations.Add(new MaterialManipulator(data.Material));
            }

            if (data.Materials != null)
            {
                manipulations.Add(new MaterialsManipulator(data.Materials));
            }

            if (data.Effect != null)
            {
                manipulations.Add(new RealtimeManipulator(data.Effect));
            }

            if (data.Texture != null)
            {
                manipulations.Add(new TextureManipulator(data.Texture));
            }

            return manipulations;
        }

        public static List<IManipulator> GetParticleChanges(string prefab)
        {
            List<IManipulator> manipulations = new List<IManipulator>();
            VisualData data = GetVisualByName(prefab);

            if (data.Particle != null)
            {
                manipulations.Add(new MaterialManipulator(data.Particle));
            }

            return manipulations;
        }
    }
}
