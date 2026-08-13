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
        private const string            PATH_FileType           = "frog";

        internal readonly static string PATH_FullDir = Path.Combine(PATH_Dir, PATH_FolderName);

        private readonly static DataContractSerializer Serializer_BingoData = new DataContractSerializer(typeof(BingoData));

        public static void Save()
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

        public static void Load()
        {
            var data_BingoData = LoadStream<BingoData>(PATH_FullDir, PATH_FileName_RoomInfo);
            if (data_BingoData != null && data_BingoData.IsVersionValid)
            {
                BingoSyncGUI.RoomID         = data_BingoData.RoomID             ?? string.Empty;
                BingoSyncGUI.Password       = data_BingoData.RoomPassword       ?? string.Empty;
                BingoSyncGUI.PlayerName     = data_BingoData.RoomPlayerName     ?? string.Empty;
                BingoSyncGUI.PlayerColor    = data_BingoData.RoomPlayerColor;
            }
        }

        private static T LoadStream<T>(string path, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(path, $"{fileName}.{PATH_FileType}");

                if (!File.Exists(fullPath))
                    return default(T);

                T data = default(T);
                using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    data = (T)Serializer_BingoData.ReadObject(stream);

                return data;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
