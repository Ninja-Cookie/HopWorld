using HarmonyLib;
using Luckshot.Input;

namespace HopWorld.Patches
{
    internal static class Patch_Init
    {
        private static bool Initialized = false;

        [HarmonyPatch(typeof(LoadManager), "AwakeIfNeeded")]
        public static class Patch_LoadManager_AwakeIfNeeded
        {
            public static void Prefix()
            {
                if (!Initialized)
                {
                    UnityEngine.GameObject.DontDestroyOnLoad(new UnityEngine.GameObject("HopWorld", typeof(BingoSyncManager.BingoSyncGUI)));
                    Initialized = true;
                }
            }
        }
    }
}
