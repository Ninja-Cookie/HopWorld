using HarmonyLib;
using System.Collections.Generic;

namespace HopWorld.Patches
{
    internal class Patch_ExposePrivateStatic : HarmonyPatch
    {
        internal static Dictionary<StringHash, CostumeData>     CostumeDataLookup;
        internal static Dictionary<StringHash, ItemData>        ItemDataLookup;
        internal static Dictionary<StringHash, DyeColorData>    DyeColorDataLookup;

        [HarmonyPatch(typeof(DevCheats), "InitializeAtRuntime")]
        public static class Patch_DevCheats_InitializeAtRuntime
        {
            public static void Postfix(Dictionary<StringHash, CostumeData> ___costumeDataLookup, Dictionary<StringHash, ItemData> ___itemDataLookup, Dictionary<StringHash, DyeColorData> ___dyeColorDataLookup)
            {
                CostumeDataLookup   = ___costumeDataLookup;
                ItemDataLookup      = ___itemDataLookup;
                DyeColorDataLookup  = ___dyeColorDataLookup;
            }
        }
    }
}
