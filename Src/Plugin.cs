using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using UnityEngine;

namespace VisualsModifier
{
    [BepInPlugin(PluginID, PluginName, PluginVersion)]
    [BepInDependency("WackyMole.WackysDatabase", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginID   = "org.bepinex.visualsmodifier";
        public const string PluginName = "Visuals Modifier";
        public const string PluginVersion = "0.1.0";

        public static readonly string Storage = Path.Combine(Paths.ConfigPath, "Visuals");

        private static ConfigSync configSync = new ConfigSync(PluginID) { };

        public static CustomSyncedValue<Dictionary<string, string>> YamlData = new(configSync, "Visual YAML", new());

        private FileSystemWatcher fileSystemWatcher;

        private Harmony _harmony;

        public Plugin()
        {
            // Create folder immediately to avoid any issues looking for the folder
            if (!Directory.Exists(Storage))
            {
                Directory.CreateDirectory(Storage);
            }

            fileSystemWatcher = new FileSystemWatcher(Path.Combine(Paths.ConfigPath, "Visuals"), "Visual_*.yml");

            fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcher.Changed += OnVisualFileChanged;
            fileSystemWatcher.Created += OnVisualFileChanged;
            fileSystemWatcher.Renamed += OnVisualFileChanged;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        [UsedImplicitly]
        public void Awake()
        {
            _harmony = new Harmony(PluginID);

            #region WackyDatabase
            Type wackyRoot = Type.GetType("wackydatabase.WMRecipeCust, WackysDatabase");

            if (wackyRoot != null)
            {
                _harmony.Patch(AccessTools.DeclaredMethod(wackyRoot, "SetItemData"), null, new HarmonyMethod(typeof(SetItemData_Patch), "Postfix"));
                _harmony.Patch(AccessTools.DeclaredMethod(wackyRoot, "GetRecipeDataFromFiles"), null, new HarmonyMethod(typeof(GetRecipeDataFromFiles_Patch), "Prefix"));
                _harmony.Patch(AccessTools.DeclaredMethod(wackyRoot, "GetAllMaterials"), null, new HarmonyMethod(typeof(GetAllMaterials_Patch), "Postfix"));
            }
            #endregion

            _harmony.PatchAll();

            YamlData.AssignLocalValue(Reload());
            YamlData.ValueChanged += VisualSyncDetected;
        }

        private Dictionary<string, string> Reload()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (string file in Directory.GetFiles(Storage, "Visual_*.yml", SearchOption.AllDirectories))
            {
                try
                {
                    map.Add(Path.GetFileName(file), File.ReadAllText(file));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[${Plugin.PluginName}]: Failed to load visual data from {file}");
                }
            }

            // Initialize the VisualData field
            VisualController.LoadVisuals(map);

            return map;
        }

        [UsedImplicitly]
        public void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        private void OnVisualFileChanged(object sender, FileSystemEventArgs args)
        {
            try
            {
                if (!configSync.IsSourceOfTruth) return;

                Debug.Log($"[{Plugin.PluginName}]: Config file changed: {args.Name}.");

                VisualData data = VisualController.Import(args.FullPath);
                VisualController.UpdateVisuals(data.PrefabName, ObjectDB.instance);

                YamlData.Value = Reload();
            } catch (Exception e)
            {
                Debug.LogError($"Detected config file change, but importing failed with an error.\n{e.Message + (e.InnerException != null ? ": " + e.InnerException.Message : "")}");
            }
        }

        private void VisualSyncDetected()
        {
            Debug.Log($"[${Plugin.PluginName}]: Config sync detected");

            VisualController.LoadVisuals(YamlData.Value);
            VisualController.Apply();

            if (!configSync.IsSourceOfTruth)
            {
                Save(YamlData.Value);
            }
        }

        public static void Save(Dictionary<string, string> visuals)
        {
            foreach (KeyValuePair<string, string> visual in visuals)
            {
                File.WriteAllText(Path.Combine(Storage, visual.Key), visual.Value);
            }
        }

        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }
}
