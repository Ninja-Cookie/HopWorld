using HarmonyLib;

namespace HopWorld.Patches
{
    internal static class Patch_Closet
    {
        [HarmonyPatch(typeof(ClosetInteractable), "CloseMenu_Async", MethodType.Enumerator)]
        public static class Patch_ClosetInteractable_CloseMenu_Async
        {
            public static void Postfix(bool __result)
            {
                if (__result)
                    return;

                Data.DataHandler.SaveCosmeticInfo();
            }
        }
    }
}
