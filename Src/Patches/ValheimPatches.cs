using HarmonyLib;

namespace VisualsModifier
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDBInit
    {
        private static void Postfix(ObjectDB __instance)
        {
            VisualController.LoadVisuals();
            VisualController.Apply();
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    public static class Console_Patch
    {
        private static void Postfix()
        {
            Terminal.ConsoleCommand VisualModifierExport = new("vm_describe", "Export visual description information for an item", args =>
            {
                string name = args[1];

                VisualController.Export(VisualController.Describe(name));                
            });

            Terminal.ConsoleCommand VisualModifierRefresh = new("vm_refresh", "Refresh materials, textures, and other effects on items", args =>
            {
                VisualController.LoadVisuals();
                VisualController.Apply();
            });
        }
    }
}
