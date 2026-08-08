using HarmonyLib;
using Luckshot.FSM;
using UnityEngine;

namespace HopWorld.Patches
{
    internal static class Patch_StateWatcher
    {
        [HarmonyPatch(typeof(StateMachine), "TriggerEnter")]
        public static class Patch_StateMachine_TriggerEnter
        {
            public static void Prefix(StateMachine __instance, Collider collider)
            {
                //if (__instance.Owner != null && __instance.Owner.GetType() != typeof(PlayerMotor) && collider.isTrigger && collider.gameObject != null && (collider.gameObject.layer == 0 || collider.gameObject.layer == 11 || collider.gameObject.layer == 25))
                //    Debug.LogError($"{collider.name}");
            }
        }
    }
}
