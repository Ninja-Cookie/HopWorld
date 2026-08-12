using Luckshot.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HopWorld.BingoSyncManager
{
    internal class BingoManager : Singleton<BingoSyncGUI>
    {
        public override bool SurvivesManagerRegen() => true;

        private static bool WasInputEnabled = false;

        public void Update()
        {
            if (InputManager.GameplayActionMap?.Gameplay != null && UnityEngine.Input.GetKeyDown(KeyCode.F3))
            {
                BingoSyncGUI.GUIOpen = !BingoSyncGUI.GUIOpen;
                BingoSyncGUI.Instance.gameObject?.SetActive(BingoSyncGUI.GUIOpen);

                if (BingoSyncGUI.GUIOpen)
                {
                    WasInputEnabled = InputManager.GameplayActionMap.Gameplay.enabled;
                    InputManager.GameplayActionMap.Gameplay.Disable();
                }
                else if (WasInputEnabled)
                {
                    InputManager.GameplayActionMap.Gameplay.Enable();
                }
            }
        }
    }
}
