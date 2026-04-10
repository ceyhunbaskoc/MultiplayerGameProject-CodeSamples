using SO;
using UnityEngine;

namespace SettingsLogic
{
    public static class DefaultSettings
    {
        private static GameSettingsSo _currentSettings;
        public static float MaxInteractionDistance => GetSettings().maxInteractionDistance;
        public static LayerMask InteractableLayer => GetSettings().interactableLayer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => GetSettings();

        private static GameSettingsSo GetSettings()
        {
            if (_currentSettings == null)
                _currentSettings = Resources.Load<GameSettingsSo>("GlobalGameSettings");
            return _currentSettings;
        }
    }
}
    