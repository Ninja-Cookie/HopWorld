using BingoSyncAPI;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using static BingoSyncAPI.BingoSyncTypes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

// ##
// Really old ugly code pulled from an earlier project. It works.
// ##

namespace HopWorld.BingoSyncManager
{
    internal static class TrueBingoSync
    {
        public static readonly BingoSync bingoSync = new BingoSync();

        public static bool IsUpdatingColor = false;

        private static List<BingoSync.MessageReceived> receivers = new List<BingoSync.MessageReceived>();

        private static void OnMessage(SocketMessage message)
        {
            if (message.type == "goal")
            {
                //SendNotification(message);
            }
            else if (message.type == "chat")
            {
                string messageText = message.text;

                if (messageText == null || messageText == string.Empty)
                    return;

                messageText = messageText.Trim();

                if (messageText.StartsWith("!"))
                {
                    messageText = messageText.Substring(1).Trim(' ');

                    string[] fullCommand = new string[1] { messageText };

                    if (fullCommand[0].Contains(' '))
                        fullCommand = messageText.Split(' ');

                    switch (fullCommand[0].ToLower())
                    {
                        case "start":
                            StartCountdown();
                        break;

                        case "ping":
                            SendMessage($"pong", true);
                        break;

                        case "pause":
                            BingoSyncGUI.Pause = BingoManager.GamePaused = true;
                            BingoManager.PauseGame();
                        break;

                        case "resume":
                            StartCountdown(true);
                        break;

                        case "help":
                            if (message.player.name.ToLower().Trim() == bingoSync.CurrentRoomInfo.PlayerName.ToLower().Trim())
                                SendMessage("[!start] Start Countdown, [!pause] Pause all players, [!resume] Resume all players, [!ping] Test players connection");
                        break;
                    }
                }
            }
        }

        private static async void StartCountdown(bool resume = false)
        {
            if (BingoManager.GamePaused && !resume)
                return;

            if (resume)
                while (BingoSyncGUI.Countdown) await Task.Yield();

            if (!BingoSyncGUI.Countdown)
            {
                BingoSyncGUI.countdownMessage   = "";
                BingoSyncGUI.Countdown          = true;

                BingoSyncGUI.Pause = false;

                int countdown = 5;

                for (int i = countdown; i >= 0; i--)
                {
                    if (i != 0)
                    {
                        //PlaySfxGameplay?.Invoke(audioManager, new object[] { SfxCollectionID.CombatSfx, AudioClipID.MineBeep, 0f });
                        BingoSyncGUI.countdownMessage = i.ToString();
                    }
                    else
                    {
                        //PlaySfxGameplay?.Invoke(audioManager, new object[] { SfxCollectionID.EnvironmentSfx, AudioClipID.MascotHit, 0f });
                        BingoSyncGUI.countdownMessage = "GO!";

                        if (resume)
                        {
                            BingoManager.GamePaused = false;
                            BingoManager.UnpauseGame();
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                BingoSyncGUI.Countdown = false;
            }
        }

        public static async void JoinRoom(RoomInfo roomInfo)
        {
            if (await bingoSync.JoinRoom(roomInfo) == BingoSync.ConnectionStatus.Connected)
            {
                RemoveReceivers();
                AddReceiver(OnMessage);
            }
            else
            {
                BingoSyncGUI.errorMessage = "Failed Connection: Check Room ID / Password";
            }
        }

        public static async void Disconnect()
        {
            if (bingoSync.Status == BingoSync.ConnectionStatus.Connected)
            {
                await bingoSync.Disconnect();
                RemoveReceivers();
            }
        }

        private static void RemoveReceivers()
        {
            for (int i = 0; i < receivers.Count; i++)
            {
                BingoSync.MessageReceived receiver = receivers[i];
                bingoSync.OnMessageReceived -= receiver;
            }
        }

        private static void AddReceiver(BingoSync.MessageReceived messageReceiver)
        {
            bingoSync.OnMessageReceived += messageReceiver;
            receivers.Add(messageReceiver);
        }

        public static async void SendMessage(string message, bool networkReply = false)
        {
            if (bingoSync.Status == BingoSync.ConnectionStatus.Connected)
            {
                if (networkReply)
                {
                    float networkTime = Time.time;
                    await bingoSync.SendChatMessage("pong");
                    message = $"{Time.time - networkTime}";
                }

                await bingoSync.SendChatMessage(message);
            }
        }
        
        public static async void SetPlayerColor(PlayerColors playerColor)
        {
            IsUpdatingColor = true;
            await bingoSync.SetPlayerColor(playerColor);
            IsUpdatingColor = false;
        }
    }
}
