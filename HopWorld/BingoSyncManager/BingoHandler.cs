using System;
using System.Collections.Generic;
using System.Linq;
using static BingoSyncAPI.BingoSyncTypes;
using static HopWorld.BingoSyncManager.TrueBingoSync;

namespace HopWorld.BingoSyncManager
{
    internal static class BingoHandler
    {
        internal enum BingoType
        {
            DISCOVER,
            MIXTAPE,
            QUEST,
            COLLECT,
            CHALLENGE,
            PHOTO,
            SHOP
        }

        internal enum BingoFilter
        {
            NONE,
            DRIPS,
            COINS,
            RANKUP,
            BITS
        }

        internal readonly static Dictionary<string, string> Quests = new Dictionary<string, string>()
        {
            {"Desert_MiniGame_Race",                                                    "Desert Race"},
            {"DusterBluffs_MetalDetector_FindMetalDetector",                            "Metal Detector"},
            {"DarkDrip_DesertPlains_TopOfTheArch",                                      "Plain Peak"},
            {"DusterBluffs_JuiceBrothers_GaveBudJuice",                                 "Stamina Juice"},
            {"DesertCanyon_ComplaintBox_DeliveredComplaintAndTalkedToBeeferAgain",      "Sinkhole Complaint"},

            {"Junktown_FloatNGo_BoatB",                                                 "Junktown Shop"},
            {"Junktown_TrashBarge_CollectTrash",                                        "Trash in Junktown"},
            {"Junktown_MiniGame_Race",                                                  "Junktown Race"},
            {"OceanRigIntro_UnsinkWedgeBoat",                                           "Wedge's Boat"},
            {"DarkDrip_OceanIntro_Peak",                                                "Island Peak"},
            {"RebelCove_Slick_AccessedBalloonFruit",                                    "Rebel Cove"},

            {"ShatteredMtn_Punkhouse_FinishedAllGraffitiANDTurnedInGraffitiQuestToKT",  "KT in Haven"},
            {"ShatteredHaven_Builder_DroppedRopeLadder",                                "Ladder in Haven"},
            {"Bug_Haven_DistantCloudIsland",                                            "Bug in Haven"}
        };

        internal readonly static Dictionary<string, string> ShopItems = new Dictionary<string, string>()
        {
            {"BoughtHat",                           "a Hat"},
            {"VeggieShopMemberCard",                "Member Card"},
            {"BackpackSlot1",                       "Backpack Slot 1"},
            {"WalletUpgrade1_BoughtFromVeggieShop", "Wallet Upgrade"},
            {"HealthUpgrade1",                      "Health Upgrade 1"},

            {"BackpackSlot2",                       "Backpack Slot 2"},
            {"HealthUpgrade2",                      "Health Upgrade 2"},
            {"HealthUpgrade3",                      "Health Upgrade 3"},

            {"BugMilestone_BugFinder",              "Find 5 Bugs for Bugsy"},
            {"BugMilestone_Heart1",                 "Find 8 Bugs for Bugsy"},
            {"BugMilestone_Hat",                    "Find 15 Bugs for Bugsy"},
            {"BugMilestone_Heart2",                 "Find 20 Bugs for Bugsy"},
            {"BugMilestone_Wallet",                 "Find 25 Bugs for Bugsy"}
        };

        internal readonly static Dictionary<string[], string> Photos = new Dictionary<string[], string>()
        {
            {new[] { "Evidence_GulleyBasement_ScamsBoard"                           }, "Sewer Basement"},
            {new[] { "Evidence_GulleyBasement_CouncilMemberPinboard"                }, "Sewer Basement"},
            {new[] { "Evidence_GulleyBasement_SinkholeMachinePrototypes"            }, "Sewer Basement"},
            {new[] { "GulleyComplaintStation"                                       }, "Complaint Station"},
            {new[] { "NPC_Copper"                                                   }, "Workshop"},

            {new[] { "NPC_Benni", "NPC_Rufus", "NPC_Welly", "NPC_Kurt", "NPC_Libby" }, "Rebel Cove"},
            {new[] { "NPC_Axton"                                                    }, "Main Rig"},

            {new[] { "NPC_Flint"                                                    }, "Mineshaft A"},
            {new[] { "NPC_Inventor"                                                 }, "Mineshaft B"},
            {new[] { "NPC_MineshaftScreamer"                                        }, "Mineshaft C"}
        };

        internal class BingoPackage
        {
            internal BingoType      Type        { get; }
            internal string         Info        { get; }
            internal string[]       InfoGroup   { get; }
            internal BingoFilter    Filter      { get; }

            internal BingoPackage(BingoType type, string info, BingoFilter bingoFilter = BingoFilter.NONE, string[] infoGroup = null)
            {
                this.Type       = type;
                this.Info       = info;
                this.Filter     = bingoFilter;
                this.InfoGroup  = infoGroup ?? Array.Empty<string>();
            }
        }

        internal static async void TryMarkSlot(BingoPackage bingoPackage)
        {
            if (bingoSync == null || bingoSync.Status != BingoSyncAPI.BingoSync.ConnectionStatus.Connected || bingoPackage == null || string.IsNullOrEmpty(bingoPackage.Info))
                return;

            List<SlotInfo>  validItemSlot   = new List<SlotInfo>();
            SlotInfo[]      itemSlots       = (await bingoSync?.GetBoardSlots())?.Where(x => x?.Info != null && !x.Colors.Contains(bingoSync.CurrentRoomInfo.PlayerColor) && x.Info.Trim().StartsWith($"[{bingoPackage.Type.ToString()}]", System.StringComparison.OrdinalIgnoreCase))?.ToArray();
            if (itemSlots == null || itemSlots.Length <= 0)
                return;

            switch (bingoPackage.Type)
            {
                case BingoType.DISCOVER:
                case BingoType.MIXTAPE:
                    var discSlot = itemSlots.FirstOrDefault(x => x.Info.ToLower().Replace($"[{bingoPackage.Type.ToString().ToLower()}]", string.Empty).Trim().Equals(bingoPackage.Info, System.StringComparison.OrdinalIgnoreCase));
                    if (discSlot != null)
                        validItemSlot.Add(discSlot);
                break;

                case BingoType.CHALLENGE:
                    var chalSlot = itemSlots.FirstOrDefault(x => string.Join("", x.Info.Trim().Split(' ')).EndsWith(bingoPackage.Info, System.StringComparison.OrdinalIgnoreCase));
                    if (chalSlot != null)
                        validItemSlot.Add(chalSlot);
                break;

                case BingoType.COLLECT:
                    if (!int.TryParse(bingoPackage.Info, out var value))
                        return;

                    var releventSlots = itemSlots.Where(x => string.Join("", x.Info.Trim().Split(' ')).EndsWith(bingoPackage.Filter.ToString(), System.StringComparison.OrdinalIgnoreCase)).ToArray();

                    foreach (var slot in releventSlots)
                    {
                        if (slot?.Info?.Trim().Split(' ').FirstOrDefault(x => int.TryParse(x, out var slotValue) && value >= slotValue) != null)
                            validItemSlot.Add(slot);
                    }
                break;

                case BingoType.SHOP:
                case BingoType.QUEST:
                    var questSlot = itemSlots.FirstOrDefault(x => x.Info.EndsWith(bingoPackage.Info, StringComparison.OrdinalIgnoreCase));
                    if (questSlot != null)
                        validItemSlot.Add(questSlot);
                break;

                case BingoType.PHOTO:
                    if (bingoPackage.InfoGroup == null || bingoPackage.InfoGroup.Length <= 0)
                        return;

                    var possiblePhoto = Photos.Keys.FirstOrDefault(x => x.All(y => bingoPackage.InfoGroup.Contains(y)));
                    if (possiblePhoto == null || possiblePhoto.Length <= 0 || !Photos.TryGetValue(possiblePhoto, out var photoQuest))
                        return;

                    var photoSlot = itemSlots.FirstOrDefault(x => x.Info.EndsWith(photoQuest, StringComparison.OrdinalIgnoreCase));
                    if (photoSlot != null)
                        validItemSlot.Add(photoSlot);
                break;
            }

            foreach (var itemSlot in validItemSlot.Where(x => x != null).ToArray())
                await bingoSync.SelectSlot(itemSlot.ID);
        }
    }
}
