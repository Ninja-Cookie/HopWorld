using HarmonyLib;
using Luckshot.Platform;

namespace HopWorld.Patches
{
    internal static class Patch_Achievements
    {
        [HarmonyPatch(typeof(PlatformServices), "TryUnlockAchievement")]
        public static class Patch_PlatformServices_TryUnlockAchievement
        {
            public static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
    }
}
