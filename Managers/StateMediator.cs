using SettingsLogic;
using UnityEngine;

namespace Managers
{
    public static class StateMediator
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GameState.OnStateChanged += HandleState;
            
            HandleState(GameState.CurrentState);
        }
        
        private static void HandleState(AppState newState)
        {
            switch (newState)
            {
                case AppState.Newspaper:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = true;
                    RuntimeVariables.CursorVisible.Value = true;
                    break;
                case AppState.Gameplay:
                    RuntimeVariables.CanProcessLookInput.Value = true;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = true;
                    RuntimeVariables.CursorVisible.Value = false;
                    RuntimeVariables.CurrentInteractionLayer.Value = LayerMask.GetMask("Environment");
                    break;
                case AppState.UIMenu:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = true;
                    RuntimeVariables.CursorVisible.Value = true;
                    break;
                case AppState.Cinematic:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = false;
                    RuntimeVariables.CursorVisible.Value = true;
                    break;
                case AppState.Paused:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = false;
                    RuntimeVariables.CursorVisible.Value = true;
                    break;
                case AppState.DocumentMode:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = true;
                    RuntimeVariables.CursorVisible.Value = true;
                    RuntimeVariables.CurrentInteractionLayer.Value = LayerMask.GetMask("DocumentModeInteractable");
                    break;
                case AppState.EndDay:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = true;
                    RuntimeVariables.CursorVisible.Value = true;
                    break;
                case AppState.Home:
                    RuntimeVariables.CanProcessLookInput.Value = false;
                    RuntimeVariables.CanPerformInteractionRaycast.Value = true;
                    RuntimeVariables.CursorVisible.Value = true;
                    break;
            }
        }
    }
}