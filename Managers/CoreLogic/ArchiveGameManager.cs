using Interact.Papers;
using Interactables;
using Managers.Network;
using UnityEngine;

namespace Managers.CoreLogic
{
    public class ArchiveGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private Drawer _drawer;

        [SerializeField] 
        private NoteMachine _noteMachine;

        [SerializeField] 
        private ArchiveBombButton _archiveBombButton;
        
        [SerializeField]
        private DangerButton _dangerButton;
        
        private bool _dayStarted = false;
        private void Awake()
        {
            CoopSyncManager.OnBroadcastReceived += _handleBroadcast;
            GameSessionManager.OnAllPlayersLoadedScene += _setupEnvironment;
        }

        private void OnDestroy()
        {
            CoopSyncManager.OnBroadcastReceived -= _handleBroadcast;
            GameSessionManager.OnAllPlayersLoadedScene -= _setupEnvironment;
        }
        
        private void _handleBroadcast(string eventName)
        {
            switch (eventName)
            {
                case CoopEvents.AllPlayersReady:    
                    if (GameState.CurrentState != AppState.Home)
                    {
                        _setupEnvironment();
                    }
                    break;
                case CoopEvents.DayEnded:
                    _dayStarted = false;
                    break;
            }
        }

        private void _setupEnvironment()
        {
            if (_dayStarted) return;
            _dayStarted = true;
            _drawer.ResetFields();
            _noteMachine.ResetFields();
            _archiveBombButton.SetInteractable(false);
            _dangerButton.SetInteractable(true);
        }

        
    }
}