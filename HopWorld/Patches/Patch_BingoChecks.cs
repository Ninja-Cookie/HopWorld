using HarmonyLib;
using Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GoalManager;
using static HopWorld.BingoSyncManager.BingoHandler;


namespace HopWorld.Patches
{
    internal static class Patch_BingoChecks
    {
        private static bool TryGetDefaultName(LString name, out string defaultName)
        {
            defaultName = string.Empty;
            string tag = name.GetValue<string>("tag");

            return !string.IsNullOrEmpty(tag) && name.HasTextInDefaultLanguage && LocalizationManager.TryGetText(tag, "en-US", out defaultName) && !string.IsNullOrEmpty(defaultName);
        }

        private static void TrySendItemData<T>(this T data, BingoType type) where T : ItemData
        {
            if (!string.IsNullOrEmpty(data?.DisplayName) && TryGetDefaultName(data.nameOverride, out var info))
                TryMarkSlot(new BingoPackage(type, info));
        }

        private static void TrySendChallenge()
        {
            string                  scene       = SceneManager.GetActiveScene().name;
            string                  info        = scene?.Split('_')?.LastOrDefault();
            List<SceneReference>    validScenes = SingletonPropertyItem<PlayerManager>.Instance?.GetValue<SceneCollection>("challengeRoomSceneCollection")?.scenes;

            if (!string.IsNullOrEmpty(scene) && !string.IsNullOrEmpty(info) && validScenes != null && validScenes.Count > 0 && validScenes.Select(x => x?.Name).Contains(scene))
                TryMarkSlot(new BingoPackage(BingoType.CHALLENGE, info));
        }

        private static void TrySendRankUpdate(bool fromRankUp)
        {
            if (!SingletonPropertyItem<PlayerManager>.Instance.TryGetPlayer(out var player) || player.Collection == null)
                return;

            var rank = player.Collection.GetDarkDripRank() - player.Collection.RankUpsOwed;
            TryMarkSlot(new BingoPackage(BingoType.COLLECT, rank.ToString(), fromRankUp ? BingoFilter.RANKUP : BingoFilter.DRIPS));
        }

        private static void TrySendCoinUpdate(int coins)
        {
            TryMarkSlot(new BingoPackage(BingoType.COLLECT, coins.ToString(), BingoFilter.COINS));
        }

        private static void TrySendBitsUpdate(int bits)
        {
            TryMarkSlot(new BingoPackage(BingoType.COLLECT, bits.ToString(), BingoFilter.BITS));
        }

        private static void TrySendQuest(string goal)
        {
            string shopItem = null;
            if (Quests.TryGetValue(goal, out var value))
                TryMarkSlot(new BingoPackage(BingoType.QUEST, value));
            else if ((shopItem = ShopItems.Keys.FirstOrDefault(x => goal.StartsWith(x))) != null && ShopItems.TryGetValue(shopItem, out string shopValue))
                TryMarkSlot(new BingoPackage(BingoType.SHOP, shopValue));
        }

        private static void TrySendPhoto(string[] subjects)
        {
            if (subjects != null && subjects.Length > 0)
                TryMarkSlot(new BingoPackage(BingoType.PHOTO, BingoType.PHOTO.ToString(), infoGroup: subjects));
        }

        [HarmonyPatch(typeof(PlayerInventory), "TryDiscoverItemData")]
        public static class Patch_PlayerInventory_TryDiscoverItemData
        {
            private static bool ItemJustFound = false;

            public static void Prefix(ItemData itemData, List<ItemData> ___discoveredItemDatas)
            {
                ItemJustFound = ___discoveredItemDatas != null && !___discoveredItemDatas.Contains(itemData);
            }

            public static void Postfix(ItemData itemData, List<ItemData> ___discoveredItemDatas)
            {
                var justFound = ItemJustFound;
                ItemJustFound = false;

                if (justFound && ___discoveredItemDatas != null && ___discoveredItemDatas.Contains(itemData))
                    itemData?.TrySendItemData(BingoType.DISCOVER);
            }
        }

        [HarmonyPatch(typeof(PlayerCollection), "TryUnlockMixtape")]
        public static class Patch_PlayerCollection_TryUnlockMixtape
        {
            public static void Prefix(PlayerCollection __instance, MixtapeData mixtape)
            {
                if (!__instance.IsMixtapeUnlocked(mixtape))
                    mixtape?.TrySendItemData(BingoType.MIXTAPE);
            }
        }

        [HarmonyPatch(typeof(PlayerMotor_HookshotDarkDrip), "FinishHookshot")]
        public static class Patch_PlayerMotor_HookshotDarkDrip_FinishHookshot
        {
            public static void Prefix(GrappleHookshot_DarkDrip ___darkDrip)
            {
                if (SingletonPropertyItem<PlayerManager>.Instance.TryGetPlayer(out var player) && player.Collection != null && !player.Collection.GetValue<List<GoalData>>("collectedDarkDrips").Contains(___darkDrip.CollectGoal))
                    TrySendChallenge();
            }
        }

        [HarmonyPatch(typeof(WidgetDarkBits), "ReflectProgress")]
        public static class Patch_WidgetDarkBits_ReflectProgress
        {
            public static void Prefix()
            {
                TrySendRankUpdate(false);
            }
        }

        [HarmonyPatch(typeof(WidgetDarkBits), "Collection_OnUseRankUp")]
        public static class Patch_WidgetDarkBits_Collection_OnUseRankUp
        {
            public static void Prefix()
            {
                TrySendRankUpdate(true);
            }
        }

        [HarmonyPatch(typeof(PlayerCollection), "ChangeCoins")]
        public static class Patch_PlayerCollection_ChangeCoins
        {
            public static void Postfix(int ___coins)
            {
                TrySendCoinUpdate(___coins);
            }
        }

        [HarmonyPatch(typeof(PlayerCollection), "CollectDarkBits")]
        public static class Patch_PlayerCollection_CollectDarkBits
        {
            private static int PreviousBits = 0;

            public static void Prefix(int ___collectedDarkBits)
            {
                PreviousBits = ___collectedDarkBits;
            }

            public static void Postfix(int ___collectedDarkBits)
            {
                if (___collectedDarkBits <= PreviousBits)
                    return;

                TrySendBitsUpdate(___collectedDarkBits);
            }
        }


        [HarmonyPatch(typeof(GoalManager), "CompleteGoal")]
        public static class Patch_GoalManager_CompleteGoal
        {
            private static bool GoalJustCompleted = false;

            public static void Prefix(Dictionary<GoalData, GoalManager.GoalState> ___goalDataToStateMap, GoalData goalData)
            {
                if (goalData != null && ___goalDataToStateMap != null && ___goalDataToStateMap.TryGetValue(goalData, out var goalState) && !goalState.isComplete)
                    GoalJustCompleted = true;
            }

            public static void Postfix(Dictionary<GoalData, GoalManager.GoalState> ___goalDataToStateMap, GoalData goalData)
            {
                bool justCompleted = GoalJustCompleted;
                GoalJustCompleted = false;

                if (!justCompleted || (goalData != null && ___goalDataToStateMap != null && ___goalDataToStateMap.TryGetValue(goalData, out var goalState) && !goalState.isComplete))
                    return;

                //Debug.LogError($"## GOAL ##: {goalData.Name}");
                TrySendQuest(goalData.Name);
            }
        }

        [HarmonyPatch(typeof(CameraGadget), "CollectPhotoSubjects")]
        public static class Patch_CameraGadget_CollectPhotoSubjects
        {
            public static void Postfix(HashSet<Item> ___itemsInPhoto)
            {
                TrySendPhoto(___itemsInPhoto?.Where(x => !string.IsNullOrEmpty(x?.name))?.Select(x => x.name).ToArray());
            }
        }
    }
}
