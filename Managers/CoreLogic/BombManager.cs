using System;
using Managers.Network;
using Unity.Netcode;
using UnityEngine;

namespace Managers.CoreLogic
{
    public class BombManager : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _bombEffect;
        
        [SerializeField]
        private DangerManager _dangerManager;

        private bool _explosionTimerStarted = false;
        private bool _randomTimerStarted = false;

        private float _randomTimer = 0f;
        private float _randomExplosionTime;

        public static Action OnBombReleased;

        private void Update()
        {
            if (_randomTimerStarted)
            {
                    _randomTimer += Time.deltaTime;
                    if (_randomTimer >= _randomExplosionTime)
                    {
                        _randomTimerStarted = false;
                        OnBombReleased?.Invoke();
                    }
            }
            else if (_explosionTimerStarted)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    if (NetworkManager.Singleton.ServerTime.Time >= CoopSyncManager.Instance.SyncedBombEndTime.Value)
                    {
                        _explosionTimerStarted = false;
                        CoopSyncManager.Instance.IsBombActive.Value = false;
                        CoopSyncManager.Instance.Broadcast(CoopEvents.BombExplode);
                        
                        if (!_dangerManager.IsDefenceActive)
                        {
                            //TODO: hospital cost will add end day summary
                        }  
                    }
                }
            }
        }

        private void Start()
        {
            CoopSyncManager.OnBroadcastReceived += _handleBroadcast;
            ReceptionGameManager.OnStartRandomBombTime += _handleOnStartRandomBombTime;
            ReceptionGameManager.OnBombReleaseImmediately += _handleOnBombReleaseImmediately;
        }
        
        private void OnDestroy()
        {
            CoopSyncManager.OnBroadcastReceived -= _handleBroadcast;
            ReceptionGameManager.OnStartRandomBombTime -= _handleOnStartRandomBombTime;
            ReceptionGameManager.OnBombReleaseImmediately -= _handleOnBombReleaseImmediately;

        }
        
        private void _handleBroadcast(string eventName)
        {
            switch (eventName)
            {
                case CoopEvents.BombButtonPressed:    _handleBombButtonPressed(); break;
            }
        }

        private void _handleBombButtonPressed()
        {
            ResetFields();
            _bombEffect.gameObject.SetActive(true);
            _bombEffect.Play();
            CoopSyncManager.Instance.Broadcast(CoopEvents.DangerEnded);
        }

        private void _handleOnStartRandomBombTime(float duration)
        {
            _randomExplosionTime = duration;
            _randomTimerStarted = true;
        }

        private void _handleOnBombReleaseImmediately()
        {
            ResetFields();
            _explosionTimerStarted = true;
            
            if (NetworkManager.Singleton.IsServer)
            {
                CoopSyncManager.Instance.IsBombActive.Value = true;
                
                CoopSyncManager.Instance.SyncedBombEndTime.Value = NetworkManager.Singleton.ServerTime.Time + CoopSyncManager.Instance.SyncedBombExplosionDuration.Value;
            }
        }

        public void ResetFields()
        {
            _explosionTimerStarted = false;
            _randomTimerStarted = false;
            _randomTimer = 0f;
            _randomExplosionTime=0;
            
            if (NetworkManager.Singleton.IsServer)
            {
                CoopSyncManager.Instance.IsBombActive.Value = false;
            }
        }
    }
}