using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace HopWorld
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid      = "ninjacookie.hops.hopworld";
        public const string PluginName      = "Hop World";
        public const string PluginVersion   = "1.0.1";

        public void Awake()
        {
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Data.DataHandler.LoadRoomInfo();
        }
    }
}
