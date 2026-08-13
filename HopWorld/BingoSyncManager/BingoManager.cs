using UnityEngine;

namespace HopWorld.BingoSyncManager
{
    internal class BingoManager : Singleton<BingoSyncGUI>
    {
        public override bool SurvivesManagerRegen() => true;

        internal static bool GamePaused = false;

        private static bool _guiOpen = false;
        public  static bool GUIOpen
        {
            get => _guiOpen;

            set
            {
                if (_guiOpen = value)
                    PauseGame();
                else if (!GamePaused)
                    UnpauseGame();
            }
        }

        private static class PanelLensInfo
        {
            private static  Lens<float>             _pauseTimeRequest;
            internal static Lens<float>             PauseTimeRequest    => _pauseTimeRequest    ?? (_pauseTimeRequest       = UpdateLens<Lens<float>>("pauseTimeRequest"));

            private static  Lens<bool>              _mouseVisibleRequest;
            internal static Lens<bool>              MouseVisibleRequest => _mouseVisibleRequest ?? (_mouseVisibleRequest    = UpdateLens<Lens<bool>>("mouseVisibleRequest"));

            private static  Lens<CursorLockMode>    _cursorLockedRequest;
            internal static Lens<CursorLockMode>    CursorLockedRequest => _cursorLockedRequest ?? (_cursorLockedRequest    = UpdateLens<Lens<CursorLockMode>>("cursorLockedRequest"));

            private static  Lens<InputLockInfo>     _inputLockedRequest;
            internal static Lens<InputLockInfo>     InputLockedRequest  => _inputLockedRequest  ?? (_inputLockedRequest     = UpdateLens<Lens<InputLockInfo>>("inputLockedRequest"));

            private static T UpdateLens<T>(string name) where T : Lens
            {
                var panel = Singleton<UIManager>.Instance?.GetPanel<PanelDevCheatConsole>();
                if (panel == null)
                    return null;

                return panel.GetValue<T>(name);
            }
        }

        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
                GUIOpen = !GUIOpen;
        }

        internal static void PauseGame()
        {
            if (PanelLensInfo.PauseTimeRequest == null || PanelLensInfo.MouseVisibleRequest == null || PanelLensInfo.CursorLockedRequest == null || PanelLensInfo.InputLockedRequest == null)
                return;

            Singleton<TimeManager>  .Instance.TimeScaleLens     .AddRequest(PanelLensInfo.PauseTimeRequest,     true);
            Singleton<UIManager>    .Instance.CursorVisibleLens .AddRequest(PanelLensInfo.MouseVisibleRequest,  true);
            Singleton<UIManager>    .Instance.CursorLockLens    .AddRequest(PanelLensInfo.CursorLockedRequest,  true);

            if (SingletonPropertyItem<PlayerManager>.Instance.TryGetPlayer(out var player))
                player.Input.LockedInputsLens.AddRequest(PanelLensInfo.InputLockedRequest, true);
        }

        internal static void UnpauseGame()
        {
            if (PanelLensInfo.PauseTimeRequest == null || PanelLensInfo.MouseVisibleRequest == null || PanelLensInfo.CursorLockedRequest == null || PanelLensInfo.InputLockedRequest == null)
                return;

            Singleton<TimeManager>  .Instance?.TimeScaleLens    .RemoveRequest(PanelLensInfo.PauseTimeRequest,      true);
            Singleton<UIManager>    .Instance?.CursorVisibleLens.RemoveRequest(PanelLensInfo.MouseVisibleRequest,   true);
            Singleton<UIManager>    .Instance?.CursorLockLens   .RemoveRequest(PanelLensInfo.CursorLockedRequest,   true);

            if (SingletonPropertyItem<PlayerManager>.Instance?.TryGetPlayer(out var player) != null)
                player.Input.LockedInputsLens.RemoveRequest(PanelLensInfo.InputLockedRequest, true);
        }
    }
}
