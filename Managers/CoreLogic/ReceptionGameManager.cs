using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Interact.Papers;
using Interactables;
using Interactables.Papers;
using Managers.Inspection;
using Managers.Network;
using Managers.Newspaper;
using Managers.UI;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers.CoreLogic
{
    public enum VerdictResult
    {
        CorrectApproval,    // ihlal yok, onayladı   ✓
        CorrectDenial,      // ihlal var, reddetti, kodlar doğru   ✓
        WrongApproval,      // ihlal var ama onayladı   ✗
        WrongDenial,        // ihlal yok ama reddetti   ✗
        UncodedenDenial,    // ihlal var, reddetti ama kod girmedi   ✗ (küçük ceza)
        WrongCode           // ihlal var, reddetti ama kodlar yanlış   ✗
    }
    public class ReceptionGameManager : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] 
        private DeskManager _deskManager;
        
        [SerializeField] 
        private GivePassportZone _giveZone;
        
        [SerializeField] 
        private CallNextGuestButton _nextGuestButton;
        
        [SerializeField] 
        private GuestSpawner _guestSpawner;
        
        [SerializeField]
        private PapersArea _papersArea;
        
        [SerializeField]
        private ReasonCodeInputUI _reasonCodeInputUI;
        
        [SerializeField]
        private NewspaperUI _newspaperUI;

        [SerializeField] 
        private BombManager _bombManager;

        [SerializeField] 
        private NoteMachine _noteMachine;
        
        [SerializeField]
        private DangerButton _dangerButton;

        [SerializeField] 
        private DoorManager _doorManager;
        
        private int _currenGuestIndex = 0;
        private List<string> _todaysQueue = new List<string>();
        
        public static Action<int> OnDayChanged;
        public static Action<int> OnGuestChanged;
        public static Action<bool> OnGuestStamped;
        public static Action<int> OnDayEnded;
        public static Action<int> OnDayStarted;
        public static Action<float> OnStartRandomBombTime;
        public static Action OnBombReleaseImmediately;
        
        public static string CurrentGuestId { get; private set; }

        public static bool IsApproved { get; set; } = false;
        
        private List<Violation> _currentViolations = new List<Violation>();

        
        public enum StampType
        {
            Waiting,
            Approved,
            Denied
        }

        private void Awake()
        {
            CallNextGuestButton.OnNextGuestRequested += _spawnCurrentGuest;
            DeskManager.OnDeskCleared += _handleOnDeskCleared;
            Passport.OnPassportStamped += _handlePassportStamped;
            CoopSyncManager.OnBroadcastReceived += _handleBroadcast;
            
            GameSessionManager.OnAllPlayersLoadedScene += _startDay;
        }

        private void OnDestroy()
        {
            CallNextGuestButton.OnNextGuestRequested -= _spawnCurrentGuest;
            DeskManager.OnDeskCleared -= _handleOnDeskCleared;
            Passport.OnPassportStamped -= _handlePassportStamped;
            CoopSyncManager.OnBroadcastReceived -= _handleBroadcast;
            
            GameSessionManager.OnAllPlayersLoadedScene -= _startDay;
        }
        
        private void _handleBroadcast(string eventName)
        {
            switch (eventName)
            {
                case CoopEvents.DayEnded:   
                case CoopEvents.BombExplode:  
                    _handleEndDayLocal();
                    _bombManager.ResetFields();
                    break;
                case CoopEvents.AllPlayersReady:
                    if (GameState.CurrentState != AppState.Home)
                    {
                        _startDay(); 
                        _setupEnvironment();
                    }
                    break;
                case CoopEvents.DangerEnded:  
                    _bombManager.ResetFields();
                    break;
            }
        }

        private bool _dayStarted = false;
        private void _startDay()
        {
            if (_dayStarted) return;
            _dayStarted = true;
            _currenGuestIndex = 0;
            
            if (NetworkManager.Singleton.IsServer)
            {
                CoopSyncManager.Instance.BroadcastStartDay();
            }
            _todaysQueue = DayManager.LoadDayQueue(CoopSyncManager.Instance.SyncedDayIndex.Value).guestQueue.ToList();

            OnDayStarted?.Invoke(CoopSyncManager.Instance.SyncedDayIndex.Value);
            OnDayChanged?.Invoke(CoopSyncManager.Instance.SyncedDayIndex.Value);
        }

        private void _setupEnvironment()
        {
            _deskManager.ClearDesk();
            _nextGuestButton.SetInteractable(true);
            _dangerButton.SetInteractable(true);
            _noteMachine.ResetFields();
            _doorManager.ResetFields();
        }
        
        /*
        private void _callNextGuest()
        {
            _deskManager.ClearDesk();
            _currenGuestIndex++;
            if (_currenGuestIndex >= _todaysQueue.Count)
            {
                CoopSyncManager.Instance.BroadcastEndDay();
                return;
            }
            _spawnCurrentGuest();
            OnGuestChanged?.Invoke(CoopSyncManager.Instance.SyncedDayIndex.Value);
        }
        */
        
        private void _handlePassportStamped(bool isApproved)
        {
            _giveZone.CanAcceptDocument = true;
            IsApproved = isApproved;
            if(_guestSpawner.CurrentActiveGuestObject.IsDangerous)
                OnBombReleaseImmediately?.Invoke();
            else
                OnGuestStamped?.Invoke(IsApproved);
        }
        
        private void _spawnCurrentGuest()
        {
            if (_currenGuestIndex >= _todaysQueue.Count)
            {
                _nextGuestButton.SetInteractable(false);
                CoopSyncManager.Instance.BroadcastEndDay();
                return;
            }
            
            _reasonCodeInputUI.ResetUI();
            
            CurrentGuestId = _todaysQueue[_currenGuestIndex];
            _giveZone.CanAcceptDocument = false;
            _nextGuestButton.SetInteractable(false);
            _guestSpawner.SpawnGuest(CurrentGuestId);
            OnGuestChanged?.Invoke(_currenGuestIndex);

            if (_guestSpawner.CurrentActiveGuestObject.IsDangerous)
            {
                float randomBombTime = Random.Range(20, 60);
                OnStartRandomBombTime?.Invoke(randomBombTime);
            }
            
            _validateCurrentGuest();
        }
        
        private void _handleOnDeskCleared()
        {
            _giveZone.CanAcceptDocument = false;
            
            _papersArea.SwitchDocumentMode(false);
            
            DOVirtual.DelayedCall(1.5f, () => 
            {
                VerdictResult verdict = _calculateVerdict();
                VerdictUI.Show(verdict, _currentViolations);
                ReasonCodeService.Clear();
                _guestSpawner.DismissGuest(IsApproved, _unlockNextGuestButton);
            });
        }
        
        private void _unlockNextGuestButton()
        {
            _nextGuestButton.SetInteractable(true);
            _currenGuestIndex++;
        }
        
        private void _handleEndDayLocal()
        {
            if(!_dayStarted) return;
            _dayStarted = false;
            OnDayEnded?.Invoke(CoopSyncManager.Instance.SyncedDayIndex.Value);
    
            if (NetworkManager.Singleton.IsServer)
            {
                CoopSyncManager.Instance.SyncedDayIndex.Value++;
                //TODO: Players should teleport to house
            }
    
            _todaysQueue.Clear();
        }

        #region Validation Logic

        private void _validateCurrentGuest()
        {
            GuestEntry guest = GuestManager.GetGuest(CurrentGuestId);
            List<Rule> activeRules = RuleManager.GetActiveRules(CoopSyncManager.Instance.SyncedDayIndex.Value);
            _currentViolations = DocumentValidator.Validate(guest, activeRules);
        }
        
        private VerdictResult _calculateVerdict()
        {
            bool hasViolations = _currentViolations.Count > 0;
            bool isApproved = IsApproved;
            List<string> enteredCodes = ReasonCodeService.EnteredCodes.ToList();
            bool hasCodes = enteredCodes.Count > 0;

            if (!hasViolations && isApproved)  return VerdictResult.CorrectApproval;
            if (!hasViolations && !isApproved) return VerdictResult.WrongDenial;
            if (hasViolations && isApproved)   return VerdictResult.WrongApproval;

            if (!hasCodes) return VerdictResult.UncodedenDenial;

            List<string> violationIds = _currentViolations.Select(v => v.RuleId).ToList();
            bool allCodesValid = enteredCodes.All(c => violationIds.Contains(c));

            return allCodesValid ? VerdictResult.CorrectDenial : VerdictResult.WrongCode;
        }

        #endregion
    }
}