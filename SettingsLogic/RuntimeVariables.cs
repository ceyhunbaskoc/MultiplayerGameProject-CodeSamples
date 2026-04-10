using UnityEngine;

namespace SettingsLogic
{
    public static class RuntimeVariables
    {
        public static readonly ReactiveProperty<bool> CursorVisible = new ReactiveProperty<bool>(false);
        public static readonly ReactiveProperty<bool> CanPerformInteractionRaycast = new ReactiveProperty<bool>(true);
        public static readonly ReactiveProperty<bool> CanProcessLookInput = new ReactiveProperty<bool>(true);
        public static readonly ReactiveProperty<bool> IsGamePaused = new ReactiveProperty<bool>(false);
        
        public static ReactiveProperty<LayerMask> CurrentInteractionLayer = new ReactiveProperty<LayerMask>();
    }
}