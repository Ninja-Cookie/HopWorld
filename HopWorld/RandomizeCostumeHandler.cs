using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HopWorld
{
    internal static class RandomizeCostumeHandler
    {
        private static PlayerItem Player
        {
            get
            {
                if (SingletonPropertyItem<PlayerManager>.Instance.TryGetPlayer(out var player))
                    return player;
                return null;
            }
        }

        private static ItemData[] _allHats;
        internal static ItemData[] AllHats
        {
            get
            {
                if (_allHats != null)
                    return _allHats;

                return _allHats = Patches.Patch_ExposePrivateStatic.ItemDataLookup?
                    .Select(x => x.Value)
                    .Where(item => item != null && Singleton<ItemManager>.Instance?
                    .GetItemPrefab(item)?
                    .GetProperty<HatItem>() != null)
                    .ToArray();
            }
        }

        private static CostumeData[] _allCostumes;
        private static CostumeData[] AllCostumes
        {
            get
            {
                if (_allCostumes != null)
                    return _allCostumes;

                return _allCostumes = Patches.Patch_ExposePrivateStatic.CostumeDataLookup?
                    .Select(x => x.Value)
                    .ToArray();
            }
        }

        private static PlayerSkinData[] _allSkins;
        private static PlayerSkinData[] AllSkins
        {
            get
            {
                if (_allSkins != null)
                    return _allSkins;

                return _allSkins = Player?.Costume?.AllSkinColors;
            }
        }

        private static DyeColorData[] _allDyes;
        private static DyeColorData[] AllDyes
        {
            get
            {
                if (_allDyes != null)
                    return _allDyes;

                return _allDyes = Patches.Patch_ExposePrivateStatic.DyeColorDataLookup?
                    .Select(x => x.Value)
                    .ToArray();
            }
        }

        private static CostumePartData[] _allTops;
        private static CostumePartData[] AllTops
        {
            get
            {
                if (_allTops != null)
                    return _allTops;

                return _allTops = AllCostumes?.Where(x => x.topPart != null).Select(x => x.topPart).ToArray();
            }
        }

        private static CostumePartData[] _allPants;
        private static CostumePartData[] AllPants
        {
            get
            {
                if (_allPants != null)
                    return _allPants;

                return _allPants = AllCostumes?.Where(x => x.bottomPart != null).Select(x => x.bottomPart).ToArray();
            }
        }

        public enum RandomCostumeType
        {
            CostumeOnly,
            Everything,
            TopOnly,
            PantsOnly,
            HatOnly,
            FrogColorOnly
        }

        private enum CostumePart
        {
            Top,
            Bottom,
            Hat,
            Color
        }

        public static void RandomizeCostume(RandomCostumeType costumeType = RandomCostumeType.CostumeOnly)
        {
            List<CostumePart> costumePartsToChange = new List<CostumePart>();
            switch (costumeType)
            {
                case RandomCostumeType.Everything:
                case RandomCostumeType.CostumeOnly:
                    costumePartsToChange.Add(CostumePart.Top);
                    costumePartsToChange.Add(CostumePart.Bottom);
                    costumePartsToChange.Add(CostumePart.Hat);
                    if (costumeType == RandomCostumeType.Everything)
                        costumePartsToChange.Add(CostumePart.Color);
                    break;

                case RandomCostumeType.TopOnly: costumePartsToChange.Add(CostumePart.Top); break;
                case RandomCostumeType.PantsOnly: costumePartsToChange.Add(CostumePart.Bottom); break;
                case RandomCostumeType.HatOnly: costumePartsToChange.Add(CostumePart.Hat); break;
                case RandomCostumeType.FrogColorOnly: costumePartsToChange.Add(CostumePart.Color); break;
            }

            foreach (var partToChange in costumePartsToChange)
                EquipRandomPart(partToChange);
        }

        private static void EquipRandomPart(CostumePart typeOfPart)
        {
            switch (typeOfPart)
            {
                case CostumePart.Top:
                case CostumePart.Bottom:
                    var part = typeOfPart == CostumePart.Top ? AllTops[UnityEngine.Random.Range(0, AllTops.Length)] : AllPants[UnityEngine.Random.Range(0, AllPants.Length)];
                    var dye = AllDyes[UnityEngine.Random.Range(0, AllDyes.Length)];
                    Player?.Costume?.SetCostumePart(part, dye);
                    break;

                case CostumePart.Hat:
                    var range = UnityEngine.Random.Range(0, AllHats.Length + 1);
                    if (range < AllHats.Length)
                        EquipHat(AllHats[range]);
                    else
                        RemovePlayerHat();
                    break;

                case CostumePart.Color: Player?.Costume?.SetBaseSkinColor(AllSkins[UnityEngine.Random.Range(0, AllSkins.Length)]); break;
            }
        }

        // Functions mimicing the same code found in-game under ClosetInteractable, SetPlayerHat and RemovePlayerHat
        // ----------------
        private static void EquipHat(ItemData hat)
        {
            RemovePlayerHat();
            if (hat == null)
                return;

            Item prefab = Singleton<ItemManager>.Instance.GetItemPrefab(hat);
            if (prefab == null)
                return;

            Item spawnedHat = null;
            if (!NetworkUtils.TrySpawn<Item>(prefab, out spawnedHat, default(Vector3), default(Quaternion), null))
                spawnedHat = UnityEngine.Object.Instantiate<Item>(prefab);

            if (spawnedHat == null)
                return;

            HatItem hatItem = spawnedHat.GetProperty<HatItem>();
            if (hatItem == null)
            {
                DestroyManager.Destroy(spawnedHat.gameObject);
                return;
            }

            hatItem.AttachToCharacter(Player);
        }

        private static void RemovePlayerHat()
        {
            if (Player == null || Player.ActiveHatAttachPoint == null || !Player.ActiveHatAttachPoint.TryGetAttachedItem(out var attachItem))
                return;

            HatItem hat = attachItem.GetComponentInParent<HatItem>();
            if (hat == null)
                return;

            hat.Detach();
            DestroyManager.Destroy(hat.gameObject);
        }
        // ----------------
    }
}
