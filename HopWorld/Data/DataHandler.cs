using BepInEx;
using HopWorld.BingoSyncManager;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using static HopWorld.Data.DataInfo;

namespace HopWorld.Data
{
    internal static class DataHandler
    {
        private readonly static string  PATH_Dir                = Paths.ConfigPath;
        private const string            PATH_FolderName         = "HopWorld";
        private const string            PATH_FileName_RoomInfo  = "roomdata";
        private const string            PATH_FileName_CosInfo   = "cosmeticdata";
        private const string            PATH_FileType           = "frog";

        internal readonly static string PATH_FullDir = Path.Combine(PATH_Dir, PATH_FolderName);

        private readonly static DataContractSerializer Serializer_BingoData = new DataContractSerializer(typeof(BingoData));
        private readonly static DataContractSerializer Serializer_CosData   = new DataContractSerializer(typeof(BingoCosmeticsData));

        public static void SaveCosmeticInfo()
        {
            if (!SingletonPropertyItem<PlayerManager>.Instance.TryGetPlayer(out var player) || player.Costume == null)
                return;

            int hat         = -1;
            int top         = -1;
            int topDye      = -1;
            int pants       = -1;
            int pantsDye    = -1;
            int skin        = -1;

            try
            {
                var hatData = player.ActiveHatAttachPoint.TryGetAttachedItem(out var attachedHat) ? attachedHat?.Item?.Data : null;
                if (hatData != null)
                    hat = Array.IndexOf(RandomizeCostumeHandler.AllHats, hatData);

                var topData = player.Costume.GetValue<CostumePartData>("costumePartTop");
                if (topData != null)
                {
                    top         = Array.IndexOf(RandomizeCostumeHandler.AllTops,    topData);
                    topDye      = Array.IndexOf(RandomizeCostumeHandler.AllDyes,    player.Costume.GetCurrentDyeColor(topData));
                }

                var pantsData = player.Costume.GetValue<CostumePartData>("costumePartBottom");
                if (pantsData != null)
                {
                    pants       = Array.IndexOf(RandomizeCostumeHandler.AllPants,   pantsData);
                    pantsDye    = Array.IndexOf(RandomizeCostumeHandler.AllDyes,    player.Costume.GetCurrentDyeColor(pantsData));
                }

                var skinData = player.Costume.GetValue<PlayerSkinData>("currentSkinColor");
                if (skinData != null)
                    skin = Array.IndexOf(RandomizeCostumeHandler.AllSkins, skinData);
            }
            catch{}

            BingoCosmeticsData data_BingoCosmeticsData = new BingoCosmeticsData(hat, top, topDye, pants, pantsDye, skin);
            SaveData(data_BingoCosmeticsData, PATH_FullDir, PATH_FileName_CosInfo, Serializer_CosData);
        }

        public static void SaveRoomInfo()
        {
            BingoData data_BingoData = null;
            var roomInfo = TrueBingoSync.bingoSync?.CurrentRoomInfo;
            if (roomInfo != null)
                data_BingoData = new BingoData(roomInfo.RoomID, roomInfo.RoomPassword, roomInfo.PlayerName, roomInfo.PlayerColor);

            SaveData(data_BingoData, PATH_FullDir, PATH_FileName_RoomInfo, Serializer_BingoData);
        }

        private static void SaveData<T>(T data, string path, string fileName, DataContractSerializer serializer)
        {
            try
            {
                if (data == null || serializer == null)
                    return;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (Directory.Exists(path))
                {
                    string fullPath = Path.Combine(path, $"{fileName}.{PATH_FileType}");
                    using (FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        serializer.WriteObject(stream, data);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static bool LoadCosmeticInfo()
        {
            var data_BingoCosmeticsData = LoadStream<BingoCosmeticsData>(PATH_FullDir, PATH_FileName_CosInfo, Serializer_CosData);
            if (data_BingoCosmeticsData != null && data_BingoCosmeticsData.IsVersionValid)
            {
                data_BingoCosmeticsData.EquipCosmetics();

                return true;
            }

            return false;
        }

        public static bool LoadRoomInfo()
        {
            var data_BingoData = LoadStream<BingoData>(PATH_FullDir, PATH_FileName_RoomInfo, Serializer_BingoData);
            if (data_BingoData != null && data_BingoData.IsVersionValid)
            {
                BingoSyncGUI.RoomID         = data_BingoData.RoomID             ?? string.Empty;
                BingoSyncGUI.Password       = data_BingoData.RoomPassword       ?? string.Empty;
                BingoSyncGUI.PlayerName     = data_BingoData.RoomPlayerName     ?? string.Empty;
                BingoSyncGUI.PlayerColor    = data_BingoData.RoomPlayerColor;

                return true;
            }

            return false;
        }

        private static T LoadStream<T>(string path, string fileName, DataContractSerializer serializer)
        {
            try
            {
                string fullPath = Path.Combine(path, $"{fileName}.{PATH_FileType}");

                if (!File.Exists(fullPath))
                    return default(T);

                T data = default(T);
                using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    data = (T)serializer.ReadObject(stream);

                return data;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
