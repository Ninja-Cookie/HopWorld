using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PanelTitle;

namespace HopWorld.Patches
{
    internal static class Patch_NewGame
    {
        private static readonly string[] ImportantGoals_Start =
        {
            "Airship",
            "BigHops",
            "Camera",
            "Compass",
            "DesertIntro",
            "FreeClimb",
            "ForestEnding",
            "GoodSwim",
            "Grind",
            "PostGame",
            "RebelCove",
            "SewerEntrance",
            "ShatteredMineShaft",
            "Void"
        };

        private static readonly string[] ImportantGoals_Any =
        {
            "GoldenPath",
            "SawIntro",
            "EndingStarted",
            "EndingTalkToWett",
            "TalkToDissAndLeaveTheWorld",
            "TalkToBenniForCove",
            "HydroPlant_Exit",
            "TreasureRoom",
            "FinaleComplete",
            "SawDissWorldOutro",
            "LockUp",
            "SawWorldIntro",
            "SawFinaleScene"
        };

        private const string Path       = "Assets/Scenes/";
        private const string Location   = "Desert_DusterBluffs";
        private const string Extension  = ".unity";

        private readonly static Vector3 StartingPosition    = new Vector3(-210.16f, 44.53f, 40.38f);
        private readonly static Vector3 StartingAngle       = new Vector3(0f, 251f, 0f);

        private static bool StartingNewGame = false;

        [HarmonyPatch(typeof(LoadUtils), "TryGetScenePathAndCheckpoint")]
        public static class Patch_LoadUtils_TryGetScenePathAndCheckpoint
        {
            public static void Postfix(WorldState loadWorldState, ref string scenePath, ref string checkpointName)
            {
                var panelTitle = Singleton<UIManager>.Instance?.GetPanel<PanelTitle>();
                if (loadWorldState != null || panelTitle == null || panelTitle.GetValue<PanelTitle.SaveWidgetMode>("saveWidgetsMode") != SaveWidgetMode.NewGame)
                    return;

                StartingNewGame = true;
                scenePath       = string.Join("", Path, Location, Extension);
                checkpointName  = string.Empty;

                Singleton<LoadManager>.Instance.SceneLoaded -= EnableCloset;
                Singleton<LoadManager>.Instance.SceneLoaded += EnableCloset;
            }

            private static void EnableCloset(string scene)
            {
                if (SceneManager.GetActiveScene().name != Location)
                    return;

                var closet = GameObject.FindObjectOfType<ClosetInteractable>(true)?.gameObject;
                if (closet != null)
                {
                    closet.SetActive(true);
                    closet.transform.position = new Vector3(-199.17f, 42.72f, 47.69f);
                    closet.transform.rotation = Quaternion.Euler(new Vector3(0.00f, 237.00f, 0.00f));
                }
            }
        }

        [HarmonyPatch(typeof(LoadManager), "MovePlayerToCheckpointOrSpawn")]
        public static class Patch_LoadManager_MovePlayerToCheckpointOrSpawn
        {
            public static void Postfix()
            {
                var newGame     = StartingNewGame;
                StartingNewGame = false;

                if (!newGame || !SingletonPropertyItem<PlayerManager>.Instance.TryGetPlayer(out var player))
                    return;

                player.Motor.SnapToLocation(StartingPosition, Quaternion.Euler(StartingAngle.x, StartingAngle.y, StartingAngle.z));
                var goals = SingletonPropertyItem<GoalManager>.Instance.GetValue<List<GoalData>>("allGoals");
                foreach (var goal in goals)
                    if (ImportantGoals_Any.Any(x => goal.Name.Contains(x)) || ImportantGoals_Start.Any(x => goal.Name.StartsWith(x)))
                        SingletonPropertyItem<GoalManager>.Instance.CompleteGoal(goal);

                foreach (KeyValuePair<StringHash, CostumeData> costumeKVP in Patch_ExposePrivateStatic.CostumeDataLookup)
                    player.Costume.UnlockCostume(costumeKVP.Value);

                foreach (ItemData hatItem in RandomizeCostumeHandler.AllHats)
                    player.Inventory.TryDiscoverItemData(hatItem);

                if (!Data.DataHandler.LoadCosmeticInfo())
                    RandomizeCostumeHandler.RandomizeCostume(RandomizeCostumeHandler.RandomCostumeType.Everything);

                SingletonPropertyItem<SaveManager>.Instance.TrySaveWorldState(null, false);
            }
        }

        [HarmonyPatch(typeof(ClosetInteractable), "ShowStartMenu")]
        public static class Patch_ClosetInteractable_ShowStartMenu
        {
            private static MethodInfo target = AccessTools.Method(typeof(NetworkUtils), "IsOnlineAndValid", new[] { typeof(Fusion.NetworkObject) });

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && instruction.operand as MethodInfo == target)
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        continue;
                    }

                    yield return instruction;
                }
            }
        }
    }
}
