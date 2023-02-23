using System;
using System.Reflection;

namespace VisualsModifier
{
    static class SetItemData_Patch
    {
        private static Type t = Type.GetType("wackydatabase.WItemData, WackysDatabase");
        private static FieldInfo fi = t.GetField("name", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        public static void Postfix(object data, ObjectDB Instant)
        {
            string name = (string) fi.GetValue(data);

            if (VisualController.GetVisualByName(name) != null)
            {
                VisualController.UpdateVisuals(name, Instant);
            }
        }
    }

    static class GetRecipeDataFromFiles_Patch
    {
        public static void Prefix()
        {
            VisualController.LoadVisuals();
        }
    }

    static class GetAllMaterials_Patch
    {
        public static void Postfix()
        {
            VisualController.LoadMaterials();
        }
    }
}
