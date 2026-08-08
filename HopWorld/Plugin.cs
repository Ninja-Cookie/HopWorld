using BepInEx;
using HarmonyLib;

namespace HopWorld
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid      = "ninjacookie.hops.hopworld";
        public const string pluginName      = "Hop World";
        public const string pluginVersion   = "1.0.0";

        public void Awake()
        {
            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }
    }
}
